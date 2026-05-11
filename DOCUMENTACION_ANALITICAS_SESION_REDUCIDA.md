# Sistema de Analiticas de Sesion - Escalatopia

Proyecto: EscalatopiaGit  
Fecha: Mayo 2026  
Autor: MuhammadAnas_MajeedRafique_Oscar_LiloLindo_Ashley_Castellviboix_Alejandro_Baldeon

## 1. Resumen ejecutivo

Este documento define un sistema de analiticas de sesion para Escalatopia con 12 metricas clave. El objetivo es medir el comportamiento real de los jugadores, detectar puntos de bloqueo o frustracion, validar el funcionamiento de las mecanicas principales y tomar decisiones de diseno apoyadas en datos. La informacion se guarda en JSON al final de cada sesion para su analisis durante el desarrollo y despues del lanzamiento.

## 2. Criterio de seleccion de metricas

Se han elegido 12 metricas porque ofrecen buena cobertura de engagement, dificultad, mecanicas core y exploracion sin volver compleja la implementacion. Tambien cumplen con el requisito academico de un minimo de 5 elementos por miembro del equipo. Cada metrica puede traducirse en una accion concreta de balance, contenido o correccion tecnica.

Ademas, se priorizaron metricas que pueden registrarse de forma automatica con eventos que ya existen en el gameplay (muerte, checkpoint, uso del gancho, finalizacion de puzzle, etc.). Esto reduce el riesgo de errores de instrumentacion y evita depender de observacion manual. El criterio central no fue recolectar "muchos datos", sino recolectar datos utiles para tomar decisiones de diseno en ciclos cortos de iteracion.

Por ultimo, la seleccion busca equilibrio entre contexto y simplicidad. Por ejemplo, registrar solo "muertes" no explica por completo la experiencia del jugador; por eso se combina con tiempo de sesion, progreso de altura, intentos de boss y nivel maximo alcanzado. Esta combinacion permite interpretar mejor si un problema viene de dificultad, de falta de comprension o de mala distribucion de objetivos.

## 3. Las 12 metricas (version resumida y clara)

Las metricas no deben analizarse de manera aislada. Su valor real aparece cuando se cruzan entre si. Un ejemplo rapido: un jugador con pocas muertes no siempre indica buena experiencia; si ademas tiene tiempo de sesion muy bajo, pocos checkpoints y poco uso del gancho, puede significar abandono temprano por desinteres o por falta de comprension.

### Categoria A: Tiempo y engagement

#### 1) Tiempo total de sesion (int, segundos)
Mide la duracion completa de la partida. Esta metrica ayuda a entender si el juego sostiene el interes: sesiones demasiado cortas suelen relacionarse con confusion, dificultad excesiva o falta de objetivos claros, mientras que sesiones largas suelen indicar mejor retencion y progreso. Durante desarrollo sirve para ajustar ritmo y densidad de contenido; despues del lanzamiento permite segmentar jugadores y planear eventos por tiempo de juego.

Como referencia inicial, se puede trabajar con tres bandas de lectura: sesiones cortas (menos de 10 minutos), sesiones medias (10 a 30 minutos) y sesiones largas (mas de 30 minutos). Estas bandas no son reglas fijas, pero ayudan a comparar builds y detectar desviaciones tras cambios de balance o tutorial.

#### 2) Altura maxima alcanzada (float, coordenada Y)
Refleja el punto mas alto alcanzado en la sesion y funciona como indicador directo de progreso en un juego de escalada. Si muchos jugadores se quedan en una franja de altura similar, puede existir un pico de dificultad o un problema de claridad en esa zona. En desarrollo ayuda a redisenar checkpoints y obstaculos; en post-lanzamiento permite analizar puntos de abandono y crear retos por zonas.

Tambien es una metrica especialmente util para comparar versiones del nivel. Si tras un ajuste de plataformas la altura media alcanzada baja de forma consistente, es una senal objetiva de que el cambio aumento friccion. Si sube demasiado, puede indicar perdida de reto.

#### 3) Checkpoints alcanzados (int)
Cuenta cuantos checkpoints visita el jugador. Sirve para validar si la distribucion de seguridad y progreso es correcta: muy pocos checkpoints pueden aumentar frustracion y demasiados pueden reducir tension. En desarrollo permite ajustar distancias y ritmo de avance; en produccion ayuda a estudiar rutas frecuentes y cuellos de botella.

La interpretacion mejora cuando se cruza con muertes y tiempo total. Un numero alto de checkpoints con muchas muertes puede reflejar progreso con sufrimiento, mientras que pocos checkpoints con mucho tiempo de sesion puede indicar exploracion o desorientacion segun el contexto del nivel.

#### 4) Tiempo promedio por puzzle (float, segundos)
Mide el tiempo medio invertido en resolver puzzles. Si el promedio es muy bajo, los puzzles pueden ser triviales; si es muy alto, pueden resultar confusos o mal explicados. Esta metrica permite calibrar tutoriales, pistas y curva de dificultad. Tras el lanzamiento ayuda a priorizar mejoras en puzzles problematicos y a definir nuevo contenido.

Como guia practica, un rango medio estable suele ser senal de claridad y reto equilibrado. Valores extremos y sostenidos (muy bajos o muy altos) justifican revisar feedback visual, pistas, orden de introduccion o dependencias entre puzzles.

### Categoria B: Dificultad y desafio

#### 5) Total de muertes (int)
Registra todas las muertes o fallos de la sesion. Es una de las senales mas utiles para balancear dificultad y detectar frustracion temprana. En desarrollo permite localizar zonas injustas o errores de tuning; en post-lanzamiento sirve para decidir ajustes de dificultad, modos alternativos y prioridades de parcheo.

Conviene registrar tambien el contexto de las muertes (zona, boss o tipo de obstaculo) cuando sea posible. Aunque la metrica base sea un contador total, esa segmentacion permite identificar con precision si el problema es mecanico, de camara, de lectura del entorno o de timing.

#### 6) Intentos de boss (diccionario por boss)
Guarda cuantos intentos necesita el jugador en cada jefe. Esta metrica muestra la dificultad relativa entre bosses y permite identificar saltos bruscos en la curva de aprendizaje. En desarrollo facilita ajustar vida, dano, patrones y ventanas de reaccion; tras el lanzamiento se usa para nerf o buff informado y para contenido competitivo.

Una lectura util es comparar intento medio entre bosses consecutivos. Si un jefe duplica o triplica de golpe los intentos del anterior en la mayoria de sesiones, normalmente existe un salto de complejidad mal escalado o una mecanica insuficientemente telegraphed.

#### 7) Tiempo para completar boss (diccionario por boss, segundos)
Mide el tiempo total desde primer intento hasta victoria de cada boss. Complementa la metrica de intentos: un boss puede requerir pocos intentos pero combates demasiado largos, o muchos intentos con duracion corta. En desarrollo ayuda a ajustar pacing de combate; despues del lanzamiento permite comparativas, rankings y eventos de tiempo.

Esta metrica tambien permite detectar desgaste. Si el tiempo total es excesivo incluso en jugadores que finalmente vencen, la experiencia puede percibirse como agotadora aunque el balance numerico sea correcto.

### Categoria C: Mecanicas core

#### 8) Usos del gancho (int)
Cuenta cuantas veces se usa la mecanica principal del juego. Si el uso es muy bajo, puede indicar que la mecanica no se entiende, no es necesaria o no esta bien integrada en el diseno del nivel. En desarrollo permite mejorar tutorializacion y situaciones de uso; en post-lanzamiento ayuda a decidir mejoras, variantes y desafios centrados en el gancho.

En un proyecto como Escalatopia, esta metrica es critica porque valida la identidad del juego. Si el jugador progresa sin usar con frecuencia el gancho, el diseno puede estar premiando rutas alternativas no previstas o desaprovechando la mecanica principal.

#### 9) Puzzles completados (int completados / int total)
Registra progreso de puzzles en formato completados frente a total disponible. Esta metrica permite evaluar claridad de reglas, accesibilidad de soluciones y consistencia de la curva de dificultad. En desarrollo sirve para ajustar pistas y orden de introduccion; despues del lanzamiento permite crear logros, contenido adicional y guias focalizadas.

El indicador completados/total es facil de comunicar en informes y presentaciones porque resume progreso sin perder contexto. Tambien permite identificar rapidamente si el problema es general (baja finalizacion de casi todos los puzzles) o puntual (1 o 2 puzzles con bloqueo alto).

### Categoria D: Exploracion y recoleccion

#### 10) Items coleccionados (int + lista de IDs)
Mide cantidad de coleccionables y que items exactos encontro el jugador. Es util para valorar nivel de exploracion, visibilidad de recompensas y efectividad del diseno de secretos. En desarrollo ayuda a recolocar objetos y ajustar rutas; en post-lanzamiento facilita eventos de coleccion, logros y economia de recompensas.

Conservar la lista de IDs evita perder trazabilidad. No solo importa cuantos items se recogen, sino cuales se quedan sistematicamente sin encontrar, porque eso revela problemas de visibilidad, incentivos o colocacion espacial.

#### 11) Movimiento por direccion (diccionario: up/down/left/right)
Cuenta inputs de movimiento por direccion y permite identificar sesgos de navegacion. Si una direccion domina de forma extrema, puede indicar un diseno demasiado forzado o controles infrautilizados. En desarrollo ayuda a revisar layout y accesibilidad; despues del lanzamiento puede alimentar heatmaps y optimizacion de flujo de jugador.

Al combinar esta metrica con altura maxima y checkpoints, se puede confirmar si el comportamiento de movimiento esperado coincide con la intencion del nivel. Si no coincide, es una senal para revisar geometria, legibilidad del camino o tutorial de control.

#### 12) Nivel maximo alcanzado (string)
Registra el nivel o acto mas alto alcanzado en la sesion. Esta metrica resume el avance global y permite detectar puntos de abandono masivos. En desarrollo sirve para corregir actos con drop elevado; en post-lanzamiento orienta roadmap, incentivos de progresion y decisiones de contenido futuro.

Es una metrica de alto valor para producto porque conecta directamente con retencion y finalizacion del juego. Permite responder preguntas clave como "en que acto se pierde mas gente" y "que porcentaje llega al final", que son esenciales para planificar mejoras de onboarding, pacing y recompensas.

## 4. Estructura de datos JSON recomendada

```json
{
  "sessionId": "SESSION_20260507_143000",
  "timestamp": "2026-05-07T14:30:00Z",
  "playerName": "Player-001",
  "timeTotalSeconds": 1250,
  "maxHeightReached": 542.3,
  "checkpointsReached": 8,
  "averageTimePerPuzzleSeconds": 85.3,
  "totalDeaths": 15,
  "bossAttempts": {
    "keys": ["BOSS_ACT1", "BOSS_ACT2"],
    "values": [5, 12]
  },
  "bossClearTimeSeconds": {
    "keys": ["BOSS_ACT1"],
    "values": [180.5]
  },
  "hookUsesCount": 87,
  "puzzlesCompleted": 5,
  "puzzlesTotal": 7,
  "itemsCollected": 12,
  "itemIds": ["COIN_001", "COIN_002", "GEM_RED_01", "KEY_CHAMBER_2"],
  "movementStats": {
    "keys": ["up", "down", "left", "right"],
    "values": [450, 200, 380, 420]
  },
  "maxLevelReached": "ACT_2_BOSS",
  "hasCompletedGame": false,
  "notes": "Abandono en Boss Act 2 tras 12 intentos"
}
```

## 5. Casos de uso durante y despues del desarrollo

### Durante el desarrollo
1. Recolectar sesiones de prueba y obtener lineas base de dificultad y progreso.
2. Detectar rapido spikes de muertes, abandono o baja comprension de mecanicas.
3. Aplicar cambios de balance y validar con nuevas rondas de sesiones.
4. Confirmar una curva de dificultad estable antes de release.

En esta fase es recomendable trabajar por iteraciones cortas: cambiar una variable de diseno, medir de nuevo y comparar contra la linea base. Asi se evita hacer muchos cambios simultaneos que luego no permiten saber que decision produjo mejora real.

### Despues del lanzamiento
1. Confirmar comportamiento real en volumen alto de jugadores.
2. Priorizar hotfixes con impacto medible en abandono o frustracion.
3. Ajustar bosses, puzzles y recompensas con evidencia.
4. Planificar eventos, DLC y mejoras de progresion basadas en datos reales.

Tambien conviene establecer revisiones periodicas (por ejemplo, semanales o quincenales) con un panel minimo de indicadores. Esto permite reaccionar con rapidez ante cambios de comportamiento y mantener una hoja de ruta de mejoras basada en impacto.

## 6. Guia rapida de implementacion tecnica

### Archivos sugeridos
- Assets/Scripts/Systems/GameSessionAnalytics.cs
- Assets/Scripts/Systems/AnalyticsManager.cs
- Assets/Scripts/Systems/AnalyticsIntegrationExample.cs

### Integracion minima
- En PlayerController: actualizar altura maxima y registrar muerte.
- En BossController: registrar intento y clear con tiempo.
- En PuzzleController: registrar puzzle resuelto con tiempo.
- En ItemPickup: registrar item recogido.

### Guardado de datos
Los JSON de sesion se guardan en ruta local de Unity (Application.persistentDataPath), dentro de una carpeta Analytics, con nombre tipo SESSION_YYYYMMDD_HHMMSS.json.

## 7. Conclusiones

El sistema propuesto ofrece una base solida y accionable para mejorar Escalatopia de forma iterativa. Las metricas cubren progreso, dificultad, dominio de mecanicas y exploracion, y permiten pasar de decisiones por intuicion a decisiones con evidencia. El coste de integracion es moderado y el retorno es alto, tanto para balance de gameplay como para planificacion de contenido futuro.

En terminos academicos, la propuesta cumple con los requisitos de justificacio tecnica y utilidad practica, porque cada metrica tiene un uso concreto durante desarrollo y otro en fase post-lanzamiento. En terminos de produccion, el sistema es escalable: puede empezar con estas 12 metricas y ampliarse con segmentacion por zona, tipo de jugador o version de build sin rehacer la arquitectura base.

## 8. Ajuste al formato de entrega

Formato de entrega recomendado: PDF explicativo del razonamiento de las metricas elegidas y su utilidad durante y despues del desarrollo completo del proyecto. Este documento ya esta preparado en estructura academica y puede exportarse directamente a PDF.
