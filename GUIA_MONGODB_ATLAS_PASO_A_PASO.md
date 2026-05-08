# Guía MongoDB Atlas - Paso a Paso Simple

## Resumen Rápido
Necesitas:
1. Crear cuenta en MongoDB Atlas
2. Crear un cluster (servidor de base de datos)
3. Crear usuario y contraseña
4. Permitir acceso desde internet (Network Access)
5. Crear la base de datos y colección

**Tiempo total: 10-15 minutos**

---

## PASO 1: Crear Cuenta en MongoDB Atlas

### 1.1 Ir al sitio web
- Abre tu navegador (Chrome, Firefox, Edge)
- Ve a: **https://www.mongodb.com/cloud/atlas**

### 1.2 Crear cuenta
- Haz clic en **"Sign Up"** (arriba a la derecha, botón verde)
- Rellena:
  - **Email**: tu correo
  - **Password**: una contraseña segura
  - **First Name**: tu nombre
  - **Last Name**: tu apellido
- Haz clic en **"Create your Atlas account"**

### 1.3 Verificar email
- MongoDB te enviará un email
- Abre tu correo y haz clic en el enlace de verificación
- Vuelve a la página de MongoDB

---

## PASO 2: Crear un Cluster (Servidor)

### 2.1 Después de verificar, verás la pantalla de "Create a Deployment"
- Haz clic en **"Create"** (el botón verde grande)

### 2.2 Elegir tipo de despliegue
- Verás tres opciones:
  - **Dedicated** (pago)
  - **Shared** (gratis, esta es la que queremos)
  - **Serverless** (otra opción)
- Elige **"Shared"** (dice "FREE" en azul)
- Haz clic en **"Create"**

### 2.3 Configurar el cluster gratuito
Se abrirá una pantalla con opciones:

**Aquí NO cambies casi nada, solo:**
1. **Cloud Provider**: Deja lo que esté seleccionado (AWS, Google Cloud, etc.)
2. **Region**: Elige el más cercano a ti
   - Si estás en Europa: **Frankfurt, Europe** o **Ireland**
   - Si estás en España: **Frankfurt** es buena opción
3. **Cluster Name**: Cambia el nombre a algo como: **EscalatopiaCluster**
4. Haz clic en **"Create Deployment"** (abajo a la derecha)

### 2.4 Esperar
- MongoDB está creando el cluster
- Verás una pantalla de carga. **Espera 2-3 minutos**
- Cuando termine, verás el cluster en la lista

---

## PASO 3: Crear Usuario y Contraseña

### 3.1 Después de crear el cluster
- Verás una pantalla que dice **"Create a database user"**
- Rellena:
  - **Username**: `escal_user` (o el nombre que quieras)
  - **Password**: `EscalatopiaPass123!` (o una contraseña segura)
  - **Confirm Password**: repite la contraseña

### 3.2 Guardar credenciales
**IMPORTANTE**: Copia esta información en un bloc de notas o archivo:
```
Usuario: escal_user
Contraseña: EscalatopiaPass123!
```
**NO pierdas esta información**, la necesitarás después.

- Haz clic en **"Create Database User"**

---

## PASO 4: Configurar Network Access (MUY IMPORTANTE)

### 4.1 Abrir Network Access
- En el menú izquierdo, busca **"Network Access"**
- Haz clic en él

### 4.2 Permitir acceso desde tu computadora
- Verás un botón **"Add IP Address"** (arriba a la derecha)
- Haz clic en él

### 4.3 Agregar tu IP
- Se abrirá una ventana emergente
- Haz clic en **"Add My Current IP Address"**
  - MongoDB automáticamente detectará tu IP
- Haz clic en **"Confirm"**

### 4.4 Permitir acceso desde cualquier lugar (para pruebas)
**(Solo para desarrollo/pruebas, después lo cierras)**

Si Network Access sigue cerrado:
- Haz clic en **"Add IP Address"** de nuevo
- Donde dice **"IP Address"**, escribe: `0.0.0.0/0`
- En **"Description"**: escribe `Development`
- Haz clic en **"Confirm"**

**Esto permite que cualquiera acceda. Para producción, lo cambias después.**

---

## PASO 5: Crear Base de Datos y Colección

### 5.1 Volver al Cluster
- En el menú izquierdo, haz clic en **"Databases"**
- Verás tu cluster: **EscalatopiaCluster**
- Haz clic en **"Connect"** (botón verde junto al cluster)

### 5.2 Conectar o gestionar
- Se abrirá una ventana
- Haz clic en **"Go to Clusters"** o directamente haz clic en el nombre del cluster

### 5.3 Crear la base de datos
- Haz clic en el botón **"+ Create Database"**
- Rellena:
  - **Database Name**: `EscalatopiaAnalytics`
  - **Collection Name**: `sessions`
- Haz clic en **"Create"**

---

## PASO 6: Obtener la Cadena de Conexión (Connection String)

### 6.1 Ir a Connect
- Busca tu cluster en la lista
- Haz clic en **"Connect"** (botón azul junto a EscalatopiaCluster)

### 6.2 Elegir método de conexión
- Se abrirá una ventana con opciones
- Haz clic en **"Drivers"** (donde dice "Select your driver")

### 6.3 Ver la conexión
- Elige **"Node.js"** en la lista (si no aparece, busca **"Node.js"** en el menú desplegable)
- Verás código de ejemplo
- **Busca la línea que empieza con `mongodb+srv://`**

Cópiala, debería verse así:
```
mongodb+srv://escal_user:PASSWORD@cluster0.mongodb.net/?retryWrites=true&w=majority
```

### 6.4 Reemplazar PASSWORD
- Donde dice `PASSWORD`, reemplaza con tu contraseña real
  - Ejemplo: `EscalatopiaPass123!`

**Resultado final:**
```
mongodb+srv://escal_user:EscalatopiaPass123!@cluster0.mongodb.net/?retryWrites=true&w=majority

mongodb+srv://anas:Anas_712066@escalatopiaanalytics.lurpmjs.mongodb.net/?appName=EscalatopiaAnalytics
```

**Guarda esta línea en un archivo, la necesitarás en el backend.**

---

## PASO 7: Verificar que Funciona

### 7.1 Ir a "Browse Collections"
- Vuelve a **"Databases"**
- Haz clic en tu cluster
- Haz clic en la base de datos **"EscalatopiaAnalytics"**
- Verás la colección **"sessions"** (vacía por ahora)

### 7.2 Insertar documento de prueba
- Haz clic en **"Insert Document"** (botón verde)
- Se abrirá un editor JSON
- Pega esto:

```json
{
  "sessionId": "TEST_20260508_001",
  "timestamp": "2026-05-08T12:00:00Z",
  "playerName": "TestPlayer",
  "timeTotalSeconds": 60,
  "notes": "Esto es una prueba"
}
```

- Haz clic en **"Insert"**
- Verás el documento guardado

**¡Listo! MongoDB está funcionando.**

---

## RESUMEN: Lo que Ya Tienes

✅ Cuenta creada en MongoDB Atlas
✅ Cluster creado: **EscalatopiaCluster**
✅ Usuario: `escal_user`
✅ Contraseña: `EscalatopiaPass123!`
✅ Network Access habilitado
✅ Base de datos: `EscalatopiaAnalytics`
✅ Colección: `sessions`
✅ Connection String: `mongodb+srv://escal_user:EscalatopiaPass123!@cluster0.mongodb.net/?retryWrites=true&w=majority`

---

## PRÓXIMOS PASOS

Ahora necesitas:
1. Crear el backend (Node.js + Express)
2. Crear los scripts de Unity (AnalyticsManager)
3. Conectar todo

¿Quieres que te haga el backend paso a paso también?
