require('dotenv').config();
const express = require('express');
const { MongoClient } = require('mongodb');
const bodyParser = require('body-parser');
const cors = require('cors');

const app = express();
app.use(cors());
app.use(bodyParser.json());

app.use((err, req, res, next) => {
  if (err && err instanceof SyntaxError && err.status === 400 && 'body' in err) {
    return res.status(400).json({ ok: false, error: 'Invalid JSON body' });
  }
  return next(err);
});

app.get('/api/health', (req, res) => {
  res.json({ ok: true, status: 'running' });
});

const uri = process.env.MONGODB_URI;
if (!uri) { console.error('Falta MONGODB_URI a .env'); process.exit(1); }

MongoClient.connect(uri, { useNewUrlParser: true, useUnifiedTopology: true })
  .then(client => {
    const db = client.db(process.env.MONGODB_DATABASE || 'EscalatopiaAnalytics');
    const sessions = db.collection(process.env.MONGODB_COLLECTION || 'sessions');

    app.post('/api/sessions', async (req, res) => {
      try {
        const doc = req.body;
        if (!doc || !doc.sessionId) return res.status(400).json({ ok: false, error: 'Missing sessionId' });
        console.log('[API] Received sessionId:', doc.sessionId);
        const result = await sessions.insertOne(doc);
        console.log('[API] Inserted into', db.databaseName + '.' + sessions.collectionName, 'insertedId=', result.insertedId);
        res.status(200).json({ ok: true, insertedId: result.insertedId });
      } catch (err) {
        console.error(err);
        res.status(500).json({ ok: false, error: err.message });
      }
    });

    const port = process.env.PORT || 3000;
    app.listen(port, () => console.log(`API listening on http://localhost:${port}`));
  })
  .catch(err => console.error('Mongo connect error', err));
