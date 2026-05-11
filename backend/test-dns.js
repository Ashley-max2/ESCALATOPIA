const dns = require('dns').promises;
dns.resolveSrv('_mongodb._tcp.escalatopiaanalytics.lurpmjs.mongodb.net')
  .then(console.log)
  .catch(err => { console.error(err); process.exit(1); });
