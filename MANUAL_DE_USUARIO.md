# Manual de Usuario — Softcoinp ERP

**Versión del manual:** 1.0  
**Última actualización:** Junio 2026  
**Propósito:** Guía de uso del sistema de administración para copropiedades (propiedad horizontal).

Este manual se actualizará conforme el proyecto crezca. Consulte la versión más reciente en el repositorio.

---

## Índice

1. [Introducción](#1-introducción)
2. [Acceso al Sistema](#2-acceso-al-sistema)
3. [Dashboard (Tablero Principal)](#3-dashboard-tablero-principal)
4. [Módulo de Unidades](#4-módulo-de-unidades)
5. [Módulo de Residentes y Propietarios](#5-módulo-de-residentes-y-propietarios)
6. [Módulo de Presupuesto](#6-módulo-de-presupuesto)
7. [Módulo de Fondo de Imprevistos](#7-módulo-de-fondo-de-imprevistos)
8. [Módulo de Cuotas y Cartera](#8-módulo-de-cuotas-y-cartera)
9. [Módulo de Configuración](#9-módulo-de-configuración)
10. [Módulo PQR](#10-módulo-pqr)
11. [Módulo de Proveedores y Contratos](#11-módulo-de-proveedores-y-contratos)
12. [Módulo de Mantenimiento y Zonas Comunes](#12-módulo-de-mantenimiento-y-zonas-comunes)
13. [Módulo de Comunicados y Notificaciones](#13-módulo-de-comunicados-y-notificaciones)
14. [Módulo de Asambleas](#14-módulo-de-asambleas)
15. [Módulo de Reservas](#15-módulo-de-reservas)
16. [Roles y Permisos](#16-roles-y-permisos)
17. [Preguntas Frecuentes](#17-preguntas-frecuentes)
18. [Glosario](#18-glosario)
19. [Módulo de Reportes y Exportaciones](#19-módulo-de-reportes-y-exportaciones)

---

## 1. Introducción

**Softcoinp ERP** es un sistema de planificación de recursos empresariales diseñado para la administración de propiedades horizontales (conjuntos residenciales, edificios de apartamentos, centros comerciales) en Colombia.

### 1.1 ¿Qué puede hacer con este sistema?

- Administrar el catálogo de unidades (apartamentos, casas, locales)
- Gestionar propietarios, arrendatarios y grupos de convivencia
- Liquidar cuotas de administración ordinarias y extraordinarias
- Registrar pagos y administrar la cartera
- Administrar el presupuesto anual y el fondo de imprevistos (Ley 675 de 2001)
- Expedir paz y salvos y estados de cuenta
- Gestionar proveedores, contratos, facturas y pagos
- Evaluar el desempeño de proveedores con scoring
- Configurar retenciones y umbrales de aprobación
- Gestionar el inventario de bienes comunes y su mantenimiento
- Programar planes de mantenimiento preventivo y generar órdenes de trabajo
- Registrar y dar seguimiento a siniestros (inundaciones, incendios, daños estructurales)
- Configurar los datos legales y financieros del conjunto

### 1.2 Requisitos técnicos

- Navegador web moderno (Chrome, Firefox, Edge, Safari)
- Conexión a internet
- Credenciales de acceso proporcionadas por el administrador del sistema

---

## 2. Acceso al Sistema

### 2.1 Iniciar sesión

1. Abra su navegador y diríjase a la URL del sistema (proporcionada por su administrador).
2. Ingrese su **correo electrónico** y **contraseña**.
3. Acepte la **Política de Protección de Datos** (Ley 1581 de 2012) marcando la casilla correspondiente.
4. Haga clic en **Iniciar Sesión**.

![Login: formulario con email, contraseña y checkbox de política de datos]

> Si olvidó su contraseña, contacte al administrador del sistema para restablecerla.

### 2.2 Aceptar invitación

Si recibió un enlace de invitación por correo electrónico:

1. Abra el enlace en su navegador.
2. Complete su **nombre completo**.
3. Establezca su **contraseña**.
4. Haga clic en **Aceptar Invitación**.

### 2.3 Cerrar sesión

Haga clic en su nombre en la esquina superior derecha y seleccione **Cerrar Sesión** o **Salir**.

---

## 3. Dashboard (Tablero Principal)

Al iniciar sesión, llegará al **Dashboard**, que ofrece una vista general del estado del conjunto.

### 3.1 Indicadores principales (KPIs)

| Indicador | Descripción |
|-----------|-------------|
| **Recaudo del Mes** | Total de pagos recibidos en el mes actual. |
| **Cartera Vencida** | Total de cuotas vencidas no pagadas. |
| **Efectivo Disponible** | Saldo en cuentas bancarias del conjunto. |
| **Ejecución Presupuestal** | Porcentaje de gastos ejecutados vs. presupuestados. |

### 3.2 Gráficos

- **Recaudo Mensual (12 meses)**: Muestra la tendencia de recaudo mes a mes.
- **Mapa de Mora**: Tabla interactiva que muestra cada unidad con su estado de pago (al día, en mora leve, mora crítica).

### 3.3 Secciones adicionales

- **Alertas**: Notificaciones importantes (unidades en mora crítica, presupuesto con desviación, etc.).
- **Próximos Eventos**: Vencimientos de cuotas, fechas de cierre contable.
- **Actividad Reciente**: Últimos movimientos registrados en el sistema.

### 3.4 Vista por rol

El dashboard se adapta según su rol:

- **Administrador**: Ve todos los indicadores y alertas.
- **Consejo**: Ve resumen financiero y aprobaciones pendientes.
- **Contador**: Ve indicadores contables y presupuestales.
- **Auditor**: Ve reportes de auditoría.
- **Residente**: Ve solo su resumen personal (saldo pendiente, intereses acumulados, días en mora).

---

## 4. Módulo de Unidades

### 4.1 Catálogo de Unidades

Acceda desde el menú lateral: **Unidades**.

Aquí encontrará el listado completo de todas las unidades del conjunto (apartamentos, casas, locales comerciales, etc.).

**La pantalla muestra:**
- Resumen de coeficientes (suma total de coeficientes de unidades activas)
- Tabla con identificador, tipo, torre/bloque, piso, coeficiente y estado

### 4.2 Registrar una nueva unidad

1. Haga clic en **Nueva Unidad**.
2. Complete los campos:

| Campo | Descripción |
|-------|-------------|
| Identificador | Nombre de la unidad (ej. "A-101", "Casa 4") |
| Tipo de Unidad | Apartamento, Casa, Local, etc. |
| Torre o Bloque | Agrupación física (ej. "Torre 1", "Bloque A") |
| Nivel o Piso | Número de piso |
| Área Privada | Metros cuadrados construidos |
| Área de Balcón | Metros cuadrados de balcón/terraza (0 si no aplica) |
| Coeficiente | Porcentaje de participación (la suma total debe dar 100%) |
| Estado | Activa, Inactiva, En Proceso, En Litigio |
| Parqueadero | ¿Tiene parqueadero privado? |
| Cuarto Útil | ¿Tiene bodega o depósito? |

3. Haga clic en **Guardar**.

> **Importante**: La suma de coeficientes de todas las unidades activas debe ser exactamente **100.0000%**. El sistema no permitirá guardar si la suma es diferente.

### 4.3 Ver detalle de una unidad

Haga clic en una unidad de la lista para ver:

- **Características físicas**: área, coeficiente, piso, etc.
- **Ocupantes actuales**: propietarios y arrendatarios asociados.
- **Complementos**: parqueadero y bodega asignados.
- **Observaciones internas**: notas administrativas.

### 4.4 Importar unidades desde Excel

Puede cargar varias unidades a la vez usando el botón **Importar desde Excel**.

1. Descargue la plantilla (formato .xlsx).
2. Complete los datos de las unidades.
3. Suba el archivo. El sistema validará los datos y mostrará un resumen de filas exitosas y errores.

---

## 5. Módulo de Residentes y Propietarios

### 5.1 Propietarios

Acceda desde el menú: **Residentes y Prop. → Propietarios**.

#### Registrar un propietario

1. Haga clic en **Nuevo Propietario**.
2. Seleccione el **tipo de propietario**:

   - **Persona Natural**: CC, CE, Pasaporte, PEP o PPT.
   - **Persona Jurídica**: NIT (el DV se calcula automáticamente).

3. Complete los datos requeridos (documento, nombre, correo, teléfono).
4. Haga clic en **Guardar**.

#### Vincular un propietario a una unidad

Desde el detalle del propietario, pestaña **Unidades Vinculadas**:

1. Haga clic en **Vincular a Unidad**.
2. Seleccione la unidad.
3. Ingrese el **porcentaje de propiedad** (útil para copropiedades).
4. Indique si es **vocero** y si **reside en la unidad**.
5. Guarde.

> Solo puede haber **un vocero activo** por unidad.

#### Transferir propiedad

Desde el detalle del propietario, seleccione **Transferir Propiedad**.

El asistente le guiará para:
1. Seleccionar el nuevo propietario (o crear uno nuevo).
2. Ingresar la fecha de transferencia.
3. Confirmar la operación.

El sistema crea automáticamente:
- El registro de la nueva vinculación.
- El historial de transferencia.
- Una notificación in-app para el nuevo propietario.

### 5.2 Arrendatarios

Acceda desde el menú: **Residentes y Prop. → Arrendatarios**.

#### Registrar un arrendatario

1. Haga clic en **Nuevo Arrendatario**.
2. Seleccione la **unidad** que ocupará.
3. Complete sus datos personales (documento, nombre, correo, teléfono).
4. Ingrese las **fechas del contrato** de arrendamiento.
5. Indique si está **autorizado para pagar la administración**.
6. Guarde.

> Solo puede haber **un arrendatario activo** por unidad.

### 5.3 Grupo de convivencia

Cada unidad puede tener registrados los miembros del hogar y mascotas. Esta información se gestiona desde el detalle de cada unidad.

---

## 6. Módulo de Presupuesto

### 6.1 Presupuesto Anual

Acceda desde el menú: **Finanzas → Presupuesto**.

#### Crear un presupuesto

1. Seleccione el **año fiscal**.
2. Haga clic en **Crear Presupuesto**.
3. Elija el método:

   - **Manual**: Ingrese valores cuenta por cuenta.
   - **Copiar del año anterior**: El sistema copia los valores del año anterior y permite aplicar un ajuste porcentual.

4. Complete los rubros de ingresos y gastos.
5. Guarde en estado **Borrador**.

#### Activar un presupuesto

Para activar un presupuesto (requiere acta de asamblea):
1. Abra el presupuesto en borrador.
2. Haga clic en **Activar**.
3. Ingrese el **número de acta** y la **fecha de aprobación** de la asamblea.

> Una vez activado, el presupuesto no puede editarse directamente. Los cambios se hacen mediante traslados o adiciones presupuestales.

#### Traslados y adiciones presupuestales

- **Traslado**: Mueve saldo entre dos cuentas del mismo grupo (Gasto a Gasto). Aprobado por el Consejo de Administración.
- **Adición**: Incrementa el techo de gastos. Requiere aprobación de Asamblea Extraordinaria.

Para registrar:
1. Vaya a la pestaña **Traslados y Adiciones**.
2. Haga clic en **Nuevo Movimiento**.
3. Seleccione tipo (Traslado o Adición), cuentas origen/destino, monto y datos del acta.

### 6.2 Vista de ejecución

El presupuesto activo muestra la ejecución en tiempo real:

- **Presupuesto Inicial**: Valor aprobado por la asamblea.
- **Adiciones**: Incrementos aprobados durante el año.
- **Traslados**: Movimientos entre rubros.
- **Presupuesto Ajustado**: Inicial + Adiciones ± Traslados.
- **Ejecutado**: Gastos reales registrados.
- **Disponible**: Presupuesto ajustado - Ejecutado.
- **% Ejecutado**: Porcentaje de ejecución.
- **Proyección**: Estimación a fin de año basada en la tendencia actual.

> El sistema muestra **alertas** cuando un rubro supera el 90% de ejecución o cuando la proyección supera el presupuesto ajustado.

---

## 7. Módulo de Fondo de Imprevistos

### 7.1 Fondo de Imprevistos (Ley 675)

Acceda desde el menú: **Finanzas → Fondo Imprevistos**.

El **Artículo 35 de la Ley 675 de 2001** establece que toda copropiedad debe constituir un fondo de imprevistos con un mínimo del **1% de los ingresos** del período.

### 7.2 Liquidar aporte mensual

1. Seleccione el **año** y **mes** a liquidar.
2. Haga clic en **Liquidar Aporte Mensual**.
3. El sistema calcula automáticamente:

   - **Base de ingresos**: Suma de ingresos del período.
   - **Porcentaje aplicado**: El configurado en el conjunto (mínimo 1%).
   - **Monto del aporte**: Base × Porcentaje / 100.

> **Tope de acumulación**: Si el saldo del fondo supera el **10% del presupuesto anual**, el sistema no generará el aporte para evitar acumulación excesiva.

### 7.3 Registrar uso del fondo

1. Haga clic en **Registrar Uso del Fondo**.
2. Complete:

   - **Monto** a retirar (no puede superar el saldo disponible).
   - **Justificación** del gasto imprevisto.
   - **Acta del Consejo de Administración** que aprobó el retiro.
   - **Fecha de aprobación**.

---



## 8. Módulo de Cuotas y Cartera

### 8.1 Períodos de Liquidación

Acceda desde el menú: **Finanzas → Facturación**.

#### Crear período de liquidación

1. Haga clic en **Nueva Liquidación**.
2. Seleccione el **período** (YYYY-MM).
3. Defina la **fecha de corte** y la **fecha de vencimiento**.
4. Guarde en estado **Borrador**.

#### Procesar liquidación

1. Abra el período en borrador.
2. Verifique los datos.
3. Haga clic en **Procesar Liquidación**.

El sistema:
- Calcula la cuota de cada unidad: `Presupuesto Mensual × (Coeficiente / 100)`.
- Aplica el ajuste de redondeo si es necesario.
- Genera las cuotas ordinarias en estado `Pendiente`.

### 8.2 Cuotas Extraordinarias

Acceda desde el menú: **Finanzas → Facturación → Cuotas Extraordinarias** (pestaña).

1. Haga clic en **Nueva Cuota Extraordinaria**.
2. Complete:

   - **Nombre**: Ej. "Impermeabilización Fachada 2026".
   - **Monto total** aprobado por la asamblea.
   - **Número de cuotas** (contados).
   - **Tipo de distribución**: Por coeficiente o grupo específico.
   - **Período de inicio**.

3. Guarde. El sistema genera automáticamente la distribución por unidad.

### 8.3 Cobros Individuales

Acceda desde el menú: **Finanzas → Facturación → Cobros Individuales** (pestaña).

Útil para:
- **Multas** (Art. 58 Ley 675)
- **Daños a bienes comunes**
- **Parqueaderos visitantes**
- **Otros cobros particulares**

1. Haga clic en **Nuevo Cobro**.
2. Seleccione la **unidad**, **tipo de cobro**, **concepto** y **monto**.
3. Si aplica, indique si está **en disputa**.

### 8.4 Cartera

Acceda desde el menú: **Finanzas → Cartera**.

Vista general del estado de la cartera del conjunto:

- **Total Facturado**: Suma de todas las cuotas emitidas.
- **Total Recaudado**: Pagos recibidos.
- **Total Pendiente**: Diferencia.
- **Tasa de Cobro**: Porcentaje de recaudo.
- **Unidades en Deuda**: Cantidad de unidades con saldo pendiente.

**Etapas de cobro:**
| Etapa | Descripción |
|-------|-------------|
| **Preventivo** | Unidades con mora de 1 a 30 días. Se envía recordatorio. |
| **Prejurídico** | Mora de 31 a 60 días. Se envía comunicación formal. |
| **Jurídico** | Mora superior a 60 días. Se inicia proceso legal. |
| **Acuerdo de Pago** | Unidades que suscribieron un acuerdo con el Consejo. |

Cada etapa se expande para ver el detalle de unidades, deuda total, días de mora y último pago.

### 8.5 Acuerdos de Pago

Acceda desde el menú: **Finanzas → Facturación → Acuerdos de Pago** (pestaña).

#### Crear un acuerdo de pago

1. Haga clic en **Nuevo Acuerdo**.
2. Seleccione la **unidad** deudora.
3. El sistema muestra las deudas pendientes (ordinarias, extraordinarias, individuales).
4. Configure:

   - **Número de cuotas** del acuerdo.
   - **% de condonación de intereses** (aprobado por el Consejo).
   - **Número de acta del Consejo**.

5. El sistema simula el acuerdo mostrando el valor de cada cuota.
6. Confirme y guarde.

#### Seguimiento del acuerdo

Desde el detalle del acuerdo puede:
- Ver el estado de cada cuota.
- **Registrar pago** de una cuota.
- **Marcar como incumplido** (default) si una cuota supera los 5 días de mora.

> Solo puede existir **un acuerdo activo** por unidad a la vez.

### 8.6 Registro de Pagos

Acceda desde el menú: **Finanzas → Facturación → Registrar Pago**.

1. Seleccione la **unidad** que realiza el pago.
2. Ingrese la **fecha**, **monto** y **medio de pago** (Efectivo, Transferencia, Cheque, Tarjeta, Online).
3. El sistema muestra un **preview de distribución** del pago:

   El orden de imputación es el siguiente:
   1. Intereses de mora capitalizados más antiguos.
   2. Capital vencido en orden cronológico.
   3. Período corriente (cuota del mes vigente).

4. Si hay excedente, se registra como **anticipo** para el período siguiente.
5. Confirme el pago.

### 8.7 Estados de Cuenta y Paz y Salvos

Acceda desde el menú: **Finanzas → Facturación → Documentos** (pestaña).

#### Estado de Cuenta

1. Seleccione la **unidad**.
2. Defina el **rango de fechas**.
3. Haga clic en **Consultar**.
4. Puede **descargar** el estado de cuenta en formato PDF.

#### Certificado de Paz y Salvo

1. Haga clic en **Emitir Paz y Salvo**.
2. Seleccione la **unidad**.
3. Defina los **días de vigencia** del certificado.
4. Confirme. El sistema verifica que la unidad no tenga deuda pendiente.

> Solo se puede emitir si la unidad **no tiene obligaciones pendientes**. El certificado incluye número único, fecha de expedición y nombre del administrador.

---

## 9. Módulo de Configuración

Acceda desde el menú inferior: **Configuración**.

### 9.1 Legal e Identidad

- **Nombre oficial del conjunto** y NIT (con DV automático).
- **Dirección**, municipio, departamento.
- **Datos del representante legal** (nombre, documento).
- **Logo del conjunto** (formato PNG o SVG).

### 9.2 Financiero

| Parámetro | Descripción |
|-----------|-------------|
| Día de Corte | Día del mes para cierre de facturación. |
| Días de Gracia | Días adicionales después del vencimiento sin interés. |
| Tasa de Interés Máxima Legal | Tasa máxima permitida por ley. |
| Tasa de Interés Aplicada | Tasa que cobra el conjunto (no puede exceder la máxima legal). |
| Inicio Año Fiscal | Mes en que comienza el año contable. |

### 9.3 Operativo

- **Total de unidades y torres** del conjunto.
- **Política de redondeo** para liquidación de cuotas.
- **Máximo de cuotas extraordinarias activas** simultáneas.
- **Porcentaje del fondo de imprevistos** (mínimo 1%).

### 9.4 Notificaciones

- **Correo remitente** para notificaciones automáticas.
- **Frecuencia de notificaciones** a morosos.
- **Plantilla de pie de firma** para comunicaciones.

### 9.5 Documentos

Gestión de documentos oficiales:

- Cargue documentos por rol (Administración, Consejo, Auditor).
- Formatos soportados: PDF.
- Descargue documentos previamente cargados.

### 9.6 Historial de Auditoría

Registro de todos los cambios realizados en la configuración:

- Parámetro modificado.
- Valor anterior y nuevo valor.
- Usuario que realizó el cambio.
- Fecha y hora.

---

## 10. Módulo PQR (Peticiones, Quejas y Reclamos)

### 10.1 ¿Qué es una PQR?

**PQR** significa **Petición, Queja o Reclamo**. Es el canal oficial de comunicación entre los residentes y la administración del conjunto. La Ley 675 de 2001 establece la obligación del administrador de atender y responder las solicitudes de los copropietarios dentro de plazos razonables.

| Tipo | Descripción | Plazo recomendado |
|------|-------------|-------------------|
| **Petición** | Solicitud de información, servicio o acción específica (ej. solicitar paz y salvo, pedir uso del salón comunal). | 5 días hábiles |
| **Queja** | Manifestación de inconformidad por conductas que afectan la convivencia (ej. ruido excesivo, mal uso de zonas comunes). | 3 días hábiles |
| **Reclamo** | Inconformidad relacionada con cobros, sanciones o decisiones administrativas (ej. cobro incorrecto en la cuota). | 10 días hábiles |

> Los plazos pueden ser ajustados por el administrador según el reglamento del conjunto.

### 10.2 Radicar una PQR

#### Desde el portal del administrador

1. Acceda al módulo desde el menú lateral. Si no aparece visible, su rol puede no tener permisos.
2. Haga clic en **Nueva PQR**.
3. Complete los campos:

| Campo | Descripción |
|-------|-------------|
| Tipo | Petición, Queja o Reclamo |
| Categoría | Cobros, Mantenimiento, Convivencia, Zonas Comunes, Administración, Otros |
| Asunto | Título breve de la solicitud |
| Descripción | Detalle completo de la situación |
| Unidad | Unidad desde la cual se radica |
| Radicante | Nombre de la persona que presenta la PQR |
| Canal | Cómo llegó la solicitud (Portal Web, Correo, Presencial o Verbal) |
| Archivos | Documentos, fotos o soportes adjuntos |

4. Si es un **reclamo relacionado con un cobro**, marque la opción y seleccione la cuota o cargo correspondiente.
5. Si es una **queja que involucra a otro residente**, registre su nombre (se mantendrá confidencial).
6. Si es una **PQR interna** (generada por la administración), márquela como interna para que no sea visible a residentes.
7. Haga clic en **Radicar**.

> El sistema asignará automáticamente un **número de radicado** único con formato `PQR-2026-06-00001` y calculará la **fecha límite de respuesta** en días hábiles.

#### Desde el portal del residente

Los residentes pueden radicar PQR desde su portal personal. Las PQR internas de la administración no aparecen en este portal.

### 10.3 Bandeja del Administrador

Acceda a la lista de PQR activas ordenadas por urgencia. Cada PQR muestra:

- **Semáforo de tiempo**: Verde (dentro del plazo), Amarillo (50-80%), Rojo (más de 80% o vencida).
- **Indicador de prioridad**: Alta, Media o Baja.
- **Número de radicado**, tipo, estado y unidad.

Puede filtrar por:
- Estado (Radicada, En Revisión, En Trámite, Respondida, Cerrada, Reabierta, Escalada)
- Tipo (Petición, Queja, Reclamo)
- Internas (mostrar/ocultar)

### 10.4 Detalle de una PQR

Al abrir una PQR, encuentra:

- **Información general**: Datos del radicante, unidad, tipo, categoría, canal.
- **Historial de estados**: Todos los cambios de estado con fecha, usuario y justificación.
- **Respuestas**: Cada respuesta emitida por la administración, con opción de marcar como definitiva o parcial.
- **Notas internas** (solo visible para Administración/Consejo/Contador): Notas del equipo que no son visibles para el residente.
- **Archivos adjuntos**: Documentos subidos por el radicante y por la administración.
- **Alertas**: Alertas generadas por vencimiento de tiempos.

### 10.5 Responder una PQR

1. Desde el detalle de la PQR, haga clic en **Responder**.
2. Redacte el texto de la respuesta.
3. Adjunte archivos de soporte si es necesario.
4. Seleccione si la respuesta es **Definitiva** (cierra la PQR) o **Parcial** (actualización de estado).
5. Si requiere que el residente confirme haber recibido la respuesta, active la opción.
6. Haga clic en **Enviar Respuesta**.

### 10.6 Cambiar estado de una PQR

| Acción | Nuevo Estado | Cuándo usarlo |
|--------|-------------|---------------|
| Revisar | En Revisión | Cuando el administrador abre la PQR para analizarla. |
| Asignar | En Trámite | Cuando se asigna a un responsable interno. |
| Responder | Respondida | Cuando se emite una respuesta formal. |
| Cerrar | Cerrada | Cuando el radicante confirmó o venció el plazo de confirmación. |
| Reabrir | Reabierta | Cuando el radicante considera insatisfactoria la respuesta (dentro de 10 días). |
| Escalar | Escalada | Cuando se requiere intervención del Consejo de Administración. |

### 10.7 Alertas automáticas

El sistema monitorea automáticamente los tiempos de respuesta:

| Umbral | Acción |
|--------|--------|
| **50% del plazo** | Se genera una alerta interna para el administrador si la PQR sigue en estado Radicada o En Revisión. |
| **80% del plazo** | Se genera una alerta y se escala al Consejo de Administración. |
| **100% (vencimiento)** | La PQR se marca automáticamente como **Escalada** y se genera una alerta crítica en el Dashboard. |

Las alertas activas se pueden consultar desde el panel de indicadores y resolver manualmente cuando la situación esté controlada.

### 10.8 Vínculo con cartera (reclamos de cobro)

Si un reclamo está relacionado con un cobro (cuota ordinaria, extraordinaria o cobro individual):

1. Al radicar, marque la opción **Vinculado a Cobro** y seleccione el cobro correspondiente.
2. El sistema vincula el reclamo al estado de cuenta de la unidad.
3. Cuando el administrador o el consejo resuelva el reclamo:
   - **Procedente**: Marque "Reclamo Resuelto = Sí". El sistema genera automáticamente una **nota de crédito** en el módulo de cuotas, ajustando el saldo sin intervención manual adicional.
   - **Improcedente**: Marque "Reclamo Resuelto = No". El cobro se mantiene y el radicante es notificado.

### 10.9 Configuración de tiempos

El administrador puede ajustar los plazos de respuesta desde la configuración del módulo:

1. Acceda a **Configuración de Tiempos PQR**.
2. Para cada tipo (Petición, Queja, Reclamo), defina los **días hábiles** de respuesta.
3. Guarde los cambios.

> Los valores por defecto son: Petición 5 días, Queja 3 días, Reclamo 10 días.

### 10.10 Panel de indicadores PQR

El administrador cuenta con un tablero de indicadores que muestra:

- **Total de PQR**: Abiertas, cerradas y escaladas.
- **Alertas activas**: Alertas pendientes por vencimiento.
- **Tiempo promedio de respuesta**: En horas, por tipo de PQR.
- **Distribución por tipo y categoría**: Gráfico de torta.
- **Tendencia mensual**: Número de radicaciones por mes.
- **Estado actual**: Cantidad de PQR en cada estado.

### 10.11 Portal del residente

Los residentes pueden:

- Ver el listado de sus PQR activas e históricas (solo las no internas).
- Consultar el detalle de cada PQR con su historial de estados y respuestas.
- **No pueden ver las notas internas** del equipo de administración.

---

## 11. Módulo de Proveedores y Contratos

Gestión integral de proveedores, contratos, facturas, pagos, evaluaciones de desempeño y configuración de retenciones.

### 11.1 Bandeja de Proveedores

**Ruta:** `Proveedores > Proveedores`

La bandeja muestra todos los proveedores registrados con filtros por estado, tipo y búsqueda por nombre o documento.

**Acciones disponibles:**
- **Nuevo Proveedor**: Crea un nuevo proveedor con toda su información.
- **Ver**: Accede al detalle del proveedor con sus contratos y evaluaciones.

**Filtros:**
- **Estado**: Todos, Activos, Inactivos.
- **Tipo**: Todos, Natural, Jurídica.
- **Búsqueda libre**: Por nombre, documento o contacto.

### 11.2 Crear Proveedor

**Ruta:** `Proveedores > Nuevo Proveedor`

| Sección | Campos Obligatorios | Descripción |
|---------|---------------------|-------------|
| **Información del Proveedor** | Tipo, Tipo Documento, Nro. Documento, Razón Social | Datos básicos de identificación. Si el tipo es "Jurídica", se habilitan los campos de Representante Legal. |
| **Contacto** | (Ninguno obligatorio) | Nombre del contacto, email, teléfono, dirección, ciudad. |
| **Representante Legal** | (Solo si Tipo = Jurídica) | Tipo y número de documento, nombre, email del representante. |

**Reglas:**
- El número de documento debe ser único por conjunto.
- Para tipos numéricos (CC, NIT), solo se permiten dígitos.
- Se puede marcar como "Proveedor Preferido" para identificación rápida.

### 11.3 Detalle del Proveedor

**Ruta:** `Proveedores > [Proveedor]`

Muestra toda la información del proveedor organizada en secciones:

- **Información del Proveedor**: Datos básicos, tipo, documento, actividad económica.
- **Contacto**: Email, teléfono, dirección, ciudad.
- **Representante Legal**: Solo si el tipo es Jurídica.
- **Contratos**: Lista de contratos asociados con valor, fechas y estado. Botón "Nuevo Contrato" para crear uno vinculado.
- **Evaluaciones**: Historial de evaluaciones con puntaje promedio y recomendación. Botón "Evaluar" para crear una nueva evaluación con scoring del 1-5 en 4 criterios.

### 11.4 Bandeja de Contratos

**Ruta:** `Proveedores > Contratos`

 Lista de todos los contratos con información resumida: número, proveedor, tipo, valor, fechas, nivel de aprobación, estado y días hasta vencimiento.

**Indicadores en tarjetas:**
- Total Contratos
- Activos
- Por Vencer (90 días)
- Con Alertas

**Filtros:**
- **Estado**: Borrador, Activo, Suspendido, Completado, Terminado, Cancelado.
- **Tipo**: Contrato de Servicios, Suministro, Obra Civil, Arrendamiento.
- **Búsqueda libre**: Por número, objeto o proveedor.

### 11.5 Crear Contrato

**Ruta:** `Proveedores > Nuevo Contrato`

| Sección | Campos Obligatorios | Descripción |
|---------|---------------------|-------------|
| **Información del Contrato** | Proveedor, Nro. Contrato, Tipo, Objeto | Seleccione el proveedor, defina el tipo y describa el objeto del contrato. |
| **Vigencia y Valores** | Valor Total, Fecha Inicio, Fecha Fin | Defina el valor, las fechas de vigencia y si es recurrente o tiene renovación automática. |

**Reglas:**
- El nivel de aprobación se determina automáticamente según los umbrales configurados.
- Para contratos con aprobación de Consejo o Asamblea, se requiere el número de acta al activar.
- Solo los contratos en estado Borrador pueden editarse o eliminarse.

### 11.6 Detalle del Contrato

**Ruta:** `Proveedores > [Contrato]`

Muestra toda la información del contrato:

- **Información del Contrato**: Proveedor, tipo, objeto, valor total, mensual, fechas, nivel de aprobación, actas.
- **Alertas**: Alertas activas de vencimiento, pólizas o renovación. Cada alerta puede resolverse individualmente.
- **Pólizas de Seguro**: Lista de pólizas asociadas. Botón "Agregar Póliza" para registrar una nueva.
- **Facturas**: Lista de facturas del proveedor con retenciones, pagos y estado.
- **Resumen**: Estado, valores, cantidad de pólizas, facturas y alertas.

**Acciones de estado:**
- **Activar** (desde Borrador): Valida que se tenga el acta de aprobación si aplica.
- **Suspender** (desde Activo): Requiere justificación.
- **Terminar** (desde Activo/Suspendido): Requiere justificación.
- **Eliminar** (solo Borrador): Elimina el contrato permanentemente.

### 11.7 Indicadores de Proveedores

**Ruta:** `Proveedores > Indicadores`

Dashboard ejecutivo con 10 KPIs:

| Indicador | Descripción |
|-----------|-------------|
| Total Proveedores | Cantidad total de proveedores registrados |
| Proveedores Activos | Proveedores con estado Activo |
| Proveedores Preferidos | Proveedores marcados como preferidos |
| Total Contratos | Cantidad total de contratos |
| Contratos Activos | Contratos en estado Activo |
| Contratos por Vencer | Contratos activos con ≤90 días para vencer |
| Facturas Pendientes | Facturas en estado Pendiente |
| Facturas Vencidas | Facturas con estado Vencida |
| Pólizas por Vencer | Pólizas con ≤30 días para vencer |
| Alertas Activas | Alertas de contratos sin resolver |

También muestra valores monetarios: Valor total de contratos activos, valor mensual, y monto de facturas pendientes.

### 11.8 Motor de Alertas de Contratos

El sistema ejecuta un servicio en segundo plano cada 6 horas que genera automáticamente:

| Tipo de Alerta | Condición | Escalada al Consejo |
|----------------|-----------|---------------------|
| Vencimiento 90 días | Contrato activo con ≤90 días para vencer | No |
| Vencimiento 30 días | Contrato activo con ≤30 días para vencer | No |
| Vencimiento 15 días | Contrato activo con ≤15 días para vencer | Sí |
| Renovación Automática | Contrato con renovación automática a punto de vencer | No |
| Póliza por Vencer | Póliza activa con ≤30 días para vencer | No |

Las alertas se pueden resolver manualmente desde el detalle del contrato. Las alertas resueltas con más de 30 días se limpian automáticamente.

### 11.9 Configuración de Retenciones

**Ruta:** `Proveedores > Contratos > Configuración de Retenciones`

Permite configurar las tarifas de retención por tipo de servicio:

| Campo | Descripción |
|-------|-------------|
| **Tipo de Servicio** | Categoría del servicio (Mantenimiento, Aseo, etc.) |
| **Tarifa Retención Fuente** | Porcentaje de retención en la fuente (ej. 2.5%) |
| **Tarifa Retención ICA** | Porcentaje de retención ICA (ej. 0.28%) |

### 11.10 Umbrales de Aprobación

**Ruta:** `Proveedores > Contratos > Umbrales de Aprobación`

Configura los rangos de valor para determinar qué nivel aprueba un contrato:

| Nivel | Descripción |
|-------|-------------|
| **Administrador** | Contratos menores al umbral mínimo del Consejo |
| **Consejo** | Contratos dentro del rango del Consejo de Administración |
| **Asamblea** | Contratos superiores al umbral del Consejo |

Si el valor del contrato no cae en ningún rango configurado, el nivel por defecto es Administrador.

---

## 12. Módulo de Mantenimiento y Zonas Comunes

Gestión integral del inventario físico de bienes comunes, planes de mantenimiento preventivo, órdenes de trabajo correctivo y registro de siniestros. Este módulo protege el patrimonio colectivo de los copropietarios y garantiza que las zonas comunes se conserven en condiciones óptimas.

### 12.1 Inventario de Bienes Comunes

**Ruta:** `Mantenimiento > Inventario`

Pantalla principal que muestra el inventario completo de bienes del conjunto. Permite buscar, filtrar y acceder al detalle de cada bien.

**Indicadores en tarjetas:**
- Total Bienes
- Operativos
- En Mantenimiento
- Fuera de Servicio

**Filtros:**
- **Estado**: Todos, Operativos, En Mantenimiento, Fuera de Servicio, Dados de Baja.
- **Categoría**: Estructura, Equipos Eléctricos, Equipos Hidráulicos, Equipos de Seguridad, Zonas Recreativas, Zonas Verdes.
- **Es Esencial**: Filtrar solo bienes esenciales (cuya afectación compromete seguridad o habitabilidad).
- **Búsqueda libre**: Por nombre, ubicación, marca o modelo.

### 12.2 Registrar Nuevo Bien Común

**Ruta:** `Mantenimiento > Nuevo Bien`

| Sección | Campos Obligatorios | Descripción |
|---------|---------------------|-------------|
| **Información del Bien** | Nombre, Categoría, Ubicación, Es Esencial | Datos de identificación y clasificación del bien. |
| **Especificaciones Técnicas** | (Ninguno obligatorio) | Marca, modelo, número de serie, fecha de adquisición, valor de adquisición, vida útil estimada. |
| **Proveedor de Referencia** | (Ninguno obligatorio) | Proveedor o fabricante de referencia para mantenimiento. |
| **Garantía** | (Ninguno obligatorio) | Indicador de garantía vigente y fecha de vencimiento. |
| **Notas** | (Ninguno obligatorio) | Observaciones sobre el estado actual del bien. |

**Reglas:**
- Los bienes esenciales (ascensores, bombas, sistemas de seguridad) generan alertas de prioridad alta al dañarse.
- Un bien fuera de servicio no puede ser reservado en el módulo de Reservas.
- Si el bien es esencial y queda fuera de servicio, la alerta se escala al Consejo de Administración.

### 12.3 Detalle del Bien Común

**Ruta:** `Mantenimiento > [Bien]`

Página de detalle del bien organizada en 5 pestañas:

#### Información
Datos completos del bien: categoría, ubicación, especificaciones, proveedor, garantía, estado actual. Incluye botón para cambiar el estado (con registro automático en el historial).

#### Fotografías
Galería de imágenes del bien en el tiempo. Permite subir nuevas fotografías con fecha de captura y descripción. Útil para documentar el deterioro o mejora progresiva.

#### Planes de Mantenimiento
Lista de planes preventivos activos para este bien. Cada plan muestra: tipo de actividad, frecuencia en días, costo estimado, proveedor preferido y fecha del próximo mantenimiento. Botón "Nuevo Plan" para crear uno vinculado.

#### Órdenes de Trabajo
Historial de todas las órdenes de trabajo (preventivas y correctivas) asociadas al bien. Incluye filtros por estado y tipo.

#### Historial de Estados
Registro cronológico de cada cambio de estado del bien con fecha, motivo y usuario responsable. Inmutable como evidencia ante reclamaciones.

### 12.4 Planes de Mantenimiento Preventivo

**Ruta:** `Mantenimiento > [Bien] > Planes > Nuevo Plan`

| Campo | Obligatorio | Descripción |
|-------|-------------|-------------|
| **Tipo de Actividad** | Sí | Lubricación, Calibración, Inspección, Limpieza, Cambio de Filtro, Cambio de Aceite, Revisión General, Prueba, Pintura, Paisajismo, Otro. |
| **Descripción** | Sí | Descripción detallada de la actividad a realizar. |
| **Frecuencia (días)** | Sí | Intervalo en días entre cada mantenimiento. El sistema genera órdenes automáticamente. |
| **Proveedor Preferido** | No | Proveedor preferido para ejecutar esta actividad. |
| **Costo Estimado** | No | Costo estimado por intervención para efectos presupuestales. |
| **Requiere Suspensión del Servicio** | Sí | Indica si el servicio debe suspenderse durante la ejecución. |
| **Horas Fuera de Servicio** | No | Estimado de tiempo fuera de servicio en horas. |

**Reglas:**
- El motor del sistema genera órdenes de trabajo automáticamente 7 días antes de la fecha programada.
- Al completar una orden preventiva, la próxima fecha se recalcula sumando la frecuencia a la fecha de ejecución real (no a la programada).
- Solo los planes activos generan órdenes automáticas.

### 12.5 Órdenes de Trabajo

**Ruta:** `Mantenimiento > Órdenes de Trabajo`

Lista de todas las órdenes de trabajo del conjunto con información resumida: número, tipo, bien, prioridad, proveedor, fechas, estado y costo.

**Indicadores en tarjetas:**
- Total Órdenes
- Pendientes de Asignación
- En Ejecución
- Completadas

**Filtros:**
- **Estado**: Pendiente de Asignación, Asignada, En Ejecución, Completada, Cancelada.
- **Tipo**: Preventivo, Correctivo.
- **Prioridad**: Emergencia, Alta, Media, Baja.
- **Origen**: Automática, Reporte del Administrador, PQR de Residente.
- **Búsqueda libre**: Por descripción, bien o proveedor.

### 12.6 Registrar Orden de Trabajo

**Ruta:** `Mantenimiento > Nueva Orden de Trabajo`

| Sección | Campos Obligatorios | Descripción |
|---------|---------------------|-------------|
| **Información de la Orden** | Tipo, Bien, Descripción, Prioridad | Seleccione si es preventiva o correctiva, el bien afectado y describa el trabajo. |
| **Asignación** | (Ninguno obligatorio) | Proveedor asignado, fecha programada de ejecución. |
| **Costos** | (Ninguno obligatorio) | Costo estimado, cuenta presupuestal del PUC a imputar. |

**Reglas:**
- Si la orden es correctiva originada desde una PQR, seleccione la PQR en el campo correspondiente.
- Las órdenes de emergencia aplican solo a bienes esenciales.
- El sistema genera automáticamente órdenes preventivas según los planes configurados.

### 12.7 Detalle de Orden de Trabajo

**Ruta:** `Mantenimiento > [Orden]`

Página de detalle con las siguientes secciones:

**Actualizar Estado:** Cambia el estado de la orden según las transiciones permitidas:
- Pendiente de Asignación → Asignada (requiere proveedor)
- Asignada → En Ejecución (se asigna fecha de inicio automáticamente)
- En Ejecución → Completada (se asigna fecha de fin automáticamente)
- Cualquier estado → Cancelada

**Asignación de Proveedor:** Seleccione o cambie el proveedor encargado de ejecutar el trabajo.

**Registro de Costos:** Ingrese el costo real de la intervención. Si supera el costo estimado en más del 20%, el sistema genera una alerta de desviación.

**Resultado:** Registre el resultado de la intervención (Resuelto, Resuelto Parcialmente, No Resuelto) con notas justificativas.

**Evidencia Fotográfica:** Suba fotografías antes y después de la intervención para documentar el trabajo realizado.

**Reglas de negocio:**
- Al completar una orden preventiva, el sistema recalcula la próxima fecha del plan sumando la frecuencia a la fecha de ejecución real.
- Al completar una orden originada desde una PQR, el sistema actualiza automáticamente el estado de la PQR a "Respondida".
- Si el costo real supera el estimado en más del 20%, se alerta al administrador antes de confirmar.

### 12.8 Panel de Fuera de Servicio

**Ruta:** `Mantenimiento > Fuera de Servicio`

Muestra todos los bienes con estado `Fuera de Servicio` organizados por prioridad. Este panel se actualiza automáticamente y sirve como tablero de alertas.

**Indicadores:**
- Total Fuera de Servicio
- Bienes Esenciales Afectados
- Promedio Días Fuera de Servicio

**Funcionalidades:**
- Acceso directo al detalle de cada bien para actualizar su estado.
- Si un bien esencial está fuera de servicio, se muestra alerta destacada con escalación al Consejo de Administración.
- Los bienes fuera de servicio aparecen bloqueados en el módulo de Reservas.

### 12.9 Registro de Siniestros

**Ruta:** `Mantenimiento > Siniestros`

Lista de todos los siniestros registrados (inundaciones, incendios, daños estructurales, fallas eléctricas u otros).

**Filtros:**
- **Estado**: Abierto, Cerrado.
- **Tipo**: Inundación, Incendio, Daño Estructural, Falla Eléctrica, Otro.
- **Búsqueda libre**: Por nombre o descripción.

### 12.10 Crear Siniestro

**Ruta:** `Mantenimiento > Nuevo Siniestro`

| Sección | Campos Obligatorios | Descripción |
|---------|---------------------|-------------|
| **Información del Siniestro** | Nombre, Tipo, Fecha de Ocurrencia | Datos básicos del evento. |
| **Daño** | (Ninguno obligatorio) | Valor total estimado del daño en COP. |
| **Seguro** | (Ninguno obligatorio) | Número de póliza, aseguradora, archivo de la póliza digitalizada. |
| **Órdenes Vinculadas** | (Ninguno obligatorio) | Seleccione las órdenes de trabajo existentes que se relacionan con este siniestro. |

**Reglas:**
- Las órdenes de trabajo vinculadas deben pertenecer al mismo conjunto.
- Una orden solo puede pertenecer a un siniestro a la vez.
- El número de póliza se puede vincular con los contratos de seguros registrados en el módulo de Proveedores.

### 12.11 Detalle de Siniestro

**Ruta:** `Mantenimiento > [Siniestro]`

Página de detalle con tres secciones:

**Información:** Datos del siniestro, tipo, fecha, valor del daño y estado. Botón para cambiar el estado (Abierto/Cerrado).

**Órdenes Vinculadas:** Lista de órdenes de trabajo asociadas al siniestro. Botón para vincular nuevas órdenes existentes.

**Datos de Seguro:** Información de la póliza de seguro, aseguradora y archivo digitalizado de la póliza.

### 12.12 Reportes de Mantenimiento

**Ruta:** `Mantenimiento > Reportes`

Permite generar reportes de mantenimientos programados para los próximos 30, 60 o 90 días.

**Funcionalidades:**
- Seleccione el período de proyección (30, 60 o 90 días).
- El reporte muestra: bien, plan, actividad, fecha programada, costo estimado y proveedor.
- Incluye el costo total estimado por período y la comparación con el saldo disponible en la cuenta presupuestal correspondiente.
- Permite exportar el reporte para impresión o envío al Consejo de Administración.

**Uso principal:**
- Planificación presupuestal: Anticipar los costos de mantenimiento para los próximos meses.
- Toma de decisiones: Evaluar si el saldo presupuestal es suficiente o si se requiere una cuota extraordinaria.
- Reporte al Consejo: Presentar la proyección de gastos en la asamblea ordinaria.

---

## 13. Módulo de Comunicados y Notificaciones

Este módulo gestiona toda la comunicación oficial entre la administración y los residentes. Está compuesto por dos subsistemas principales: los **comunicados formales** (circulares, avisos, boletines) y las **notificaciones automáticas** (alertas generadas por eventos de otros módulos).

### 13.1 Comunicados

Los comunicados son documentos formales que la administración envía a los residentes. Pueden ser inmediatos o programados, y soportan múltiples canales de envío simultáneos.

#### Crear un nuevo comunicado

1. En el menú lateral, expanda **Comunicaciones** y seleccione **Nuevo Comunicado**.
2. Complete los siguientes campos:
   - **Asunto**: Título del comunicado (obligatorio).
   - **Contenido**: Cuerpo del comunicado.
   - **Segmentación**: Seleccione la audiencia:
     - *Todos los Propietarios*: envía a todos los propietarios registrados.
     - *Todos los Residentes*: envía a propietarios y arrendatarios.
     - *Unidades Específicas* / *Torres Específicas*: segmentación por unidad o torre.
   - **Canales**: Seleccione uno o más canales de envío (Correo, SMS, Push, Cartelera).
   - **Programación**: Opcional. Si no define fecha/hora, el comunicado se guarda como borrador.
3. Opciones adicionales:
   - *Requiere confirmación de lectura*: el destinatario debe confirmar haber leído el comunicado.
   - *Publicar en cartelera digital*: el comunicado también aparece en la cartelera.
4. Puede **Guardar Borrador** (para editar después) o **Enviar Ahora**.

#### Programar un comunicado

Si define una fecha y hora futura, el comunicado se guarda en estado `Programado`. El sistema lo enviará automáticamente a la hora programada. Mientras esté programado puede editarlo o cancelarlo desde la lista de comunicados.

#### Seguimiento de entregas

Desde el detalle del comunicado puede ver:
- **Estado por destinatario**: para cada destinatario se muestra el estado en cada canal (Correo, SMS, Push).
- **Confirmaciones de lectura**: cuántos destinatarios han confirmado la lectura.
- **Reenviar a no confirmados**: si el comunicado requiere confirmación, puede reenviarlo solo a quienes no han confirmado.

#### Archivar comunicados

Los comunicados enviados pueden archivarse (ocultarse de la vista activa) pero nunca eliminarse del sistema.

### 13.2 Plantillas de Notificación

Las plantillas definen el contenido de las notificaciones automáticas generadas por eventos de otros módulos.

1. Vaya a **Comunicaciones → Plantillas**.
2. Para crear una plantilla:
   - **Nombre**: identificador de la plantilla.
   - **Tipo de Evento**: seleccione el evento que activará esta plantilla (ej. "Pago Confirmado", "Reserva Aprobada").
   - **Para**: si la notificación va al propietario, arrendatario o ambos.
   - **Asunto / Cuerpo Email**: contenido del correo electrónico.
   - **Texto SMS**: versión para SMS (máximo 160 caracteres).
   - **Variables dinámicas**: nombres de variables que serán reemplazadas automáticamente. Use `{Propietario}`, `{Unidad}`, `{Valor}`, etc. según la plantilla.
3. Las plantillas pueden activarse o desactivarse.

### 13.3 Cartelera Digital

La cartelera digital es un mural de publicaciones visible para todos los residentes autenticados.

#### Vista de residente

Los residentes ven las publicaciones activas ordenadas por fecha (las fijadas al tope aparecen primero). Las publicaciones vencidas se ocultan automáticamente.

#### Administración de cartelera

1. Vaya a **Comunicaciones → Cartelera** y cambie a la vista *Administrar*.
2. Para crear una publicación:
   - **Título** y **Contenido** de la publicación.
   - **Categoría**: Administrativo, Financiero, Convivencia, Eventos o Urgente.
   - **Fijar al tope**: la publicación siempre aparece primero.
   - **Vence el**: fecha opcional después de la cual la publicación se archiva automáticamente.
3. Las publicaciones archivadas no se eliminan, pueden consultarse activando *Incluir archivadas*.

### 13.4 Preferencias de Comunicación

Cada residente puede configurar qué canales desea usar para recibir notificaciones.

1. Vaya a **Comunicaciones → Preferencias**.
2. Seleccione un residente y edite sus preferencias:
   - **Correo electrónico**: activar/desactivar.
   - **SMS**: activar/desactivar.
   - **Notificación Push**: activar/desactivar.
   - **Recibir notificaciones críticas**: override para emergencias (siempre activo por defecto).
3. Puede registrar **notas** sobre solicitudes de desuscripción.

> [!NOTE]
> Las notificaciones críticas (emergencias, cortes de servicios, convocatorias de asamblea) se envían por todos los canales sin importar las preferencias individuales.

### 13.5 Secuencia de Avisos de Mora

Configura la progresión automática de avisos para unidades en mora.

1. Vaya a **Comunicaciones → Secuencia Mora**.
2. Cada paso (Primer Aviso, Segundo Aviso, Tercer Aviso, Prejurídico) se configura con:
   - **Días después de vencimiento**: cuándo se activa este paso.
   - **Plantilla**: qué plantilla de notificación usar.
   - **Activo**: si el paso está habilitado.
3. Para **pausar la secuencia** para una unidad específica (ej. por acuerdo de pago):
   - Use el formulario *Nueva Pausa* indicando el ID de la unidad, motivo y fechas.
4. **Ejecutar Proceso de Mora**: procesa manualmente la secuencia para todas las unidades vencidas.

### 13.6 Eventos que generan notificaciones automáticas

| Módulo | Evento | Descripción |
|--------|--------|-------------|
| Cartera | Pago Confirmado | Notifica al propietario que su pago fue registrado. |
| Cartera | Nueva Liquidación | Avisa que la cuota mensual está disponible. |
| Cartera | Avisos de Mora | Secuencia de 4 avisos progresivos (1, 5, 15, 30 días). |
| Cartera | Acuerdo de Pago | Confirmación de acuerdo formalizado. |
| Cartera | Cuota por Vencer | Recordatorio de cuota de acuerdo próxima a vencer. |
| Cartera | Paz y Salvo | Notificación de expedición de paz y salvo. |
| PQR | Radicación | Confirmación con número de radicado. |
| PQR | Actualización de Estado | Cambio de estado de la PQR. |
| PQR | Respuesta Disponible | El administrador respondió la PQR. |
| PQR | Cierre | La PQR fue cerrada. |
| Reservas | Aprobada | La reserva fue aprobada. |
| Reservas | Rechazada | La reserva fue rechazada con motivo. |
| Reservas | Recordatorio 24h | Recordatorio un día antes. |
| Reservas | Recordatorio 2h | Recordatorio dos horas antes. |
| Reservas | Depósito Devuelto | Confirmación de devolución del depósito. |
| Asambleas | Convocatoria | Convocatoria formal con orden del día. |
| Asambleas | Recordatorio 72h | Recordatorio tres días antes. |
| Asambleas | Acta Publicada | Acta de asamblea disponible. |
| Mantenimiento | Programado | Aviso de mantenimiento que afecta servicios. |
| Mantenimiento | Fuera de Servicio | Bien fuera de servicio que afecta zonas reservadas. |
| Mantenimiento | Orden Resuelta | Resolución de orden originada desde PQR. |

---

## 14. Módulo de Asambleas

La Ley 675 de 2001 establece que toda copropiedad debe celebrar asambleas generales de propietarios al menos una vez al año (asamblea ordinaria) y adicionalmente cuando se requiera (asambleas extraordinarias). Este módulo gestiona todo el ciclo de vida de las asambleas: convocatoria, registro de asistencia, orden del día, votación, constancias, generación de actas, publicación y propagación de decisiones a otros módulos.

### 14.1 Ciclo de vida de una asamblea

El sistema maneja los siguientes estados:

| Estado | Descripción |
|--------|-------------|
| **Borrador** | La asamblea ha sido creada pero aún no se ha convocado. |
| **Convocada** | Se ha enviado la convocatoria a los propietarios. |
| **En Sesión** | La sesión está activa. |
| **Cerrada** | La sesión ha finalizado, lista para generar acta. |
| **Acta Aprobada** | El acta ha sido aprobada por el Consejo o Comisión. |
| **Publicada** | El acta ha sido publicada y notificada a los propietarios. |

**Transiciones de estado permitidas:**

```
Borrador → Convocada → En Sesión → Cerrada → Acta Aprobada → Publicada
```

### 14.2 Lista de asambleas

**Ruta:** `Asambleas`

Acceda desde el menú lateral. La página muestra el listado completo de asambleas con:

**Indicadores en tarjetas:**
- Total de asambleas
- Ordinarias
- Extraordinarias
- Publicadas

**Filtros disponibles:**
- **Estado**: Borrador, Convocada, En Sesión, Cerrada, Acta Aprobada, Publicada.
- **Tipo**: Ordinaria, Extraordinaria.
- **Búsqueda libre**: Por título o lugar.

**Tabla de asambleas:**
| Columna | Descripción |
|---------|-------------|
| Título | Nombre de la asamblea. |
| Tipo | Ordinaria o Extraordinaria. |
| Estado | Estado actual con badge de color. |
| Fecha | Fecha programada. |
| Lugar | Ubicación de la asamblea. |
| Asistentes | Número de asistentes registrados. |
| Quórum | Indicador de quórum alcanzado en primera convocatoria. |

**Acción:** "Ver" para acceder al detalle de la asamblea.

### 14.3 Crear una nueva asamblea

**Ruta:** `Asambleas > Nueva Asamblea`

1. Haga clic en **Nueva Asamblea**.
2. Complete los campos:

| Sección | Campos | Obligatorio |
|---------|--------|-------------|
| **Información General** | Título, Tipo (Ordinaria/Extraordinaria), Modalidad (Presencial/Remota/Híbrida), Descripción | Título |
| **Fecha y Lugar** | Fecha, Hora, Lugar | Todos |
| **Segunda Convocatoria** | Fecha, Hora, Lugar (opcional) | — |

3. Haga clic en **Crear Asamblea**.

> La asamblea se crea en estado **Borrador**. Para continuar, debe **Convocarla** desde el detalle.

### 14.4 Detalle de la asamblea

**Ruta:** `Asambleas > [Asamblea]`

La página de detalle está organizada en **7 pestañas**:

| Pestaña | Descripción |
|---------|-------------|
| **Información** | Datos generales: tipo, fecha, lugar, coeficiente total, umbrales de quórum, presidente y secretario. |
| **Convocatoria** | Gestión de convocatorias: crear, enviar y hacer seguimiento de entrega. |
| **Asistencia** | Registro de asistentes, control de quórum y gestión de representantes. |
| **Orden del Día** | Puntos a tratar, registro de votación por punto y resultados. |
| **Constancias** | Registro de constancias presentadas por los asistentes. |
| **Acta** | Generación automática, revisión, aprobación y publicación del acta. |
| **Propagación** | Seguimiento de la propagación de decisiones aprobadas a otros módulos. |

**Botones de acción contextuales (según estado):**

| Estado de la asamblea | Botón disponible |
|-----------------------|------------------|
| Borrador | **Convocar** |
| Convocada | **Iniciar Sesión** |
| En Sesión | **Cerrar Sesión** |
| Cerrada | **Generar Acta** |

### 14.5 Convocatoria

Desde la pestaña **Convocatoria** puede:

1. **Crear una convocatoria**:
   - Número de convocatoria (1, 2, etc.)
   - Canal de envío: Email, WhatsApp, SMS.
   - Asunto y notas adicionales.

2. **Enviar la convocatoria**: El sistema distribuye la convocatoria a todos los propietarios según el canal seleccionado.

3. **Seguimiento de entrega**: Para cada destinatario se muestra:
   - Nombre del propietario y unidad.
   - Estado de entrega: Entregado (verde) o No entregado (rojo).

> Solo puede haber una convocatoria activa a la vez. La segunda convocatoria se crea después de la primera si no se alcanza quórum.

### 14.6 Registro de asistencia

**Ruta:** `Asambleas > [Asamblea] > Asistencia`

#### Panel de quórum

Muestra en tiempo real:
- **Porcentaje de quórum**: coeficiente presente / coeficiente total.
- **Indicador**: "Quórum Logrado" o "Quórum No Logrado" para primera y segunda convocatoria.
- **Resumen**: Propietarios presentes, ausentes, con mora y con voto restringido.

#### Registrar asistencia

1. Seleccione la **unidad/propietario** de la lista.
2. Indique si asiste **personalmente** o mediante **representante**:
   - Si es representante: ingrese nombre y documento del representante, y opcionalmente el documento de poder.
3. Opciones adicionales:
   - **Miembro de comisión**: marque y seleccione el rol (Presidente, Secretario, Vocal).
   - **Notas**: campo opcional.
4. Haga clic en **Registrar Asistencia**.

#### Tabla de asistentes

| Columna | Descripción |
|---------|-------------|
| Unidad | Identificador de la unidad. |
| Propietario | Nombre del propietario. |
| Coeficiente | Porcentaje de participación. |
| Tipo | Personal o Representante. |
| Estado | Presente, Retirado. |
| Mora | Si/No (si la unidad está en mora). |
| Voto | Habilitado o Restringido. |

**Acciones sobre asistentes:**
- **Salida**: Registra la salida del propietario con hora.
- **Levantar restricción**: Permite habilitar el voto de un propietario con voto restringido, registrando el motivo.

### 14.7 Orden del día y votación

**Ruta:** `Asambleas > [Asamblea] > Orden del Día`

#### Crear un punto en la orden del día

1. Vaya a la pestaña **Orden del Día**.
2. Haga clic en **Nuevo Punto**.
3. Complete:

| Campo | Descripción |
|-------|-------------|
| **Título** | Nombre del punto a tratar. |
| **Descripción** | Detalle del punto (opcional). |
| **Presentador** | Persona que presenta el punto (opcional). |
| **Mayoría Requerida** | Simple (>50%), Calificada (>=2/3) o Unanimidad (100%). |
| **Modo de Votación** | Por Coeficiente, Aplausos, Escrito o Electrónico. |
| **Solo Informativo** | Marcar si el punto es informativo (no requiere votación). |

#### Registrar votación

1. Haga clic en **Registrar Voto** en el punto correspondiente.
2. Ingrese los resultados:

| Campo | Descripción |
|-------|-------------|
| Votos a Favor (Coeficiente) | Suma de coeficientes de votos a favor. |
| Votos en Contra (Coeficiente) | Suma de coeficientes de votos en contra. |
| Abstenciones (Coeficiente) | Suma de coeficientes de abstenciones. |
| Cantidad Votos a Favor | Número de votos a favor. |
| Cantidad Votos en Contra | Número de votos en contra. |
| Cantidad Abstenciones | Número de abstenciones. |
| Observaciones | Notas adicionales (opcional). |

3. Confirme. El sistema muestra el resultado con indicador visual (Aprobado/Rechazado).

> Para mayoría calificada, el sistema verifica que los votos a favor representen al menos 2/3 de los coeficientes presentes.

### 14.8 Constancias

**Ruta:** `Asambleas > [Asamblea] > Constancias`

Las constancias son manifestaciones formales que un propietario solicita que queden registradas en el acta.

#### Registrar una constancia

1. Seleccione el **propietario** de la lista de asistentes.
2. Opcional: vincule la constancia a un **punto del orden del día** específico.
3. Redacte el **texto** de la constancia.
4. Haga clic en **Agregar Constancia**.

### 14.9 Gestión del acta

**Ruta:** `Asambleas > [Asamblea] > Acta`

El acta es el documento oficial que resume todo lo ocurrido en la asamblea.

#### Generar acta

Cuando la asamblea está en estado **Cerrada**:
1. Vaya a la pestaña **Acta**.
2. Complete:
   - **Nombre del Presidente**.
   - **Nombre del Secretario**.
   - **Miembros de Comisión** (separados por coma, opcional).
3. Haga clic en **Generar Acta**.

El sistema genera automáticamente el acta con los datos de la asamblea, asistentes, resultados de votación y constancias.

#### Flujo de aprobación del acta

| Estado | Acción disponible |
|--------|-------------------|
| Generada | **Enviar a Revisión** o **Aprobar** directamente. |
| En Revisión | La comisión revisa y puede **Aprobar**. |
| Aprobada | **Publicar** el acta para los propietarios. |
| Publicada | El acta es visible para todos los propietarios. Notificaciones enviadas. |

**Opciones durante la revisión:**
- Fecha límite de revisión.
- Notas y comentarios de la comisión.
- **Carga de firmas**: Suba las imágenes de las firmas del Presidente y el Secretario (formato PNG).

> Una vez publicada, el acta no puede modificarse. Se notifica automáticamente a todos los propietarios.

### 14.10 Propagación de decisiones

**Ruta:** `Asambleas > [Asamblea] > Propagación`

Cuando un punto del orden del día es aprobado, sus decisiones pueden propagarse a otros módulos del sistema:

| Módulo destino | Ejemplo |
|----------------|---------|
| **Presupuesto** | Aprobación de traslados o adiciones presupuestales. |
| **Cuota Extraordinaria** | Creación de una cuota extraordinaria aprobada en asamblea. |
| **Roles y Permisos** | Cambios en la junta directiva o consejo. |
| **Asiento Contable** | Registro contable de una decisión. |
| **Contrato** | Aprobación de un contrato que requería visto de asamblea. |

La tabla de propagación muestra:
- Punto del orden del día.
- Módulo destino.
- Descripción de la propagación.
- Estado: Pendiente, Propagada o Fallida.
- Fecha y mensaje de error si falló.

### 14.11 Sesión en vivo

**Ruta:** `Asambleas > [Asamblea] > Sesión`

Página dedicada para gestionar la asamblea durante su desarrollo en tiempo real:

- **Panel de información**: fecha, hora, presidente, secretario, convocatoria.
- **Orden del día completo**: cada punto con opción de registrar voto y ver resultados con barras de progreso.
- **Registro de constancias**: formulario rápido vinculado a la lista de asistentes.
- Los resultados de votación se muestran con indicadores visuales de umbral (línea punteada para mayoría calificada).

### 14.12 Reportes

El sistema permite generar un reporte consolidado de asambleas para un período determinado, útil para el Informe Anual de Gestión.

---

## 15. Módulo de Reservas

Este módulo gestiona la reserva de espacios comunes del conjunto (salón social, piscina, zonas BBQ, canchas, gimnasio, etc.). Los residentes pueden solicitar reservas y la administración las aprueba, gestiona check-in/check-out y administra depósitos de garantía.

### 15.1 Ciclo de vida de una reserva

| Estado | Descripción |
|--------|-------------|
| **Pendiente** | Solicitud creada, esperando aprobación del administrador. |
| **Aprobada** | Reserva confirmada, pendiente de check-in. |
| **En Uso** | El espacio está siendo ocupado (check-in realizado). |
| **Completada** | Reserva finalizada normalmente (check-out realizado). |
| **Cancelada** | Cancelada por el administrador o el residente. |
| **Rechazada** | Rechazada por el administrador. |
| **Con Incidente** | Se reportó un incidente durante el uso. |

**Transiciones de estado:**

```
Pendiente → Aprobada → En Uso → Completada
    \          \          \
     \          \          +→ Con Incidente → Completada
      \          \
       \          +→ Cancelada
        \
         +→ Rechazada
```

### 15.2 Estados del depósito de garantía

| Estado | Descripción |
|--------|-------------|
| **No Requerido** | El espacio no requiere depósito. |
| **Pendiente** | Depósito requerido pero aún no pagado. |
| **Pagado** | Depósito pagado por el residente. |
| **Devuelto** | Depósito devuelto al residente (sin novedad). |
| **Aplicado a Daño** | Depósito aplicado a cubrir daños. |

### 15.3 Lista de reservas

**Ruta:** `Reservas > Reservas`

Muestra todas las reservas del conjunto en tarjetas con:

**Filtros:**
- **Búsqueda libre**: Por número de reserva, espacio, unidad o propietario.
- **Estado**: Pendiente, Aprobada, En Uso, Completada, Cancelada, Rechazada, Con Incidente.
- **Rango de fechas**: Desde / Hasta.

**Cada tarjeta muestra:**
- Número de reserva y estado (con badge de color).
- Espacio, unidad y propietario.
- Fecha y hora de inicio/fin.
- Costo total y estado del depósito.
- Asistentes estimados.

### 15.4 Crear una nueva reserva

**Ruta:** `Reservas > Nueva Reserva`

1. Seleccione el **Espacio** a reservar.
2. Seleccione la **Unidad** que realiza la reserva (búsqueda por identificador o propietario).
3. Defina la **fecha y hora de inicio** y **fin**.
4. Ingrese el número de **asistentes estimados**.
5. Complete los detalles del evento:
   - **Descripción del evento** (opcional).
   - **Incluye música**: si aplica, especifique la hora de finalización.
6. Acepte las **reglas del espacio**.
7. El sistema verifica automáticamente la **disponibilidad** en tiempo real y muestra el **costo estimado** + **depósito requerido**.
8. Haga clic en **Crear Reserva**.

> Si la unidad tiene mora, el sistema muestra una advertencia. Dependiendo de la configuración del espacio, puede bloquear la reserva o solo advertir.

### 15.5 Detalle de la reserva

**Ruta:** `Reservas > [Reserva]`

#### Información general

| Campo | Descripción |
|-------|-------------|
| Espacio | Nombre del espacio reservado. |
| Unidad | Unidad que realizó la reserva. |
| Propietario | Nombre y email del propietario. |
| Inicio / Fin | Fecha y hora del evento. |
| Asistentes | Número estimado. |
| Música | Sí/No con hora de finalización. |
| Descripción | Notas del evento. |

#### Costos

| Campo | Descripción |
|-------|-------------|
| Costo Total | Valor de la reserva. |
| Estado Depósito | Estado actual del depósito. |
| Monto Depósito | Valor del depósito de garantía. |

#### Acciones por estado

| Estado de la reserva | Acciones disponibles |
|----------------------|---------------------|
| **Pendiente** | Aprobar, Rechazar (con motivo). |
| **Aprobada** | Check-In, Cancelar. |
| **En Uso** | Check-Out, Reportar Incidente. |
| **Completada** | Gestión del depósito (si aplica). |

#### Gestión del depósito

| Estado del depósito | Acción disponible |
|---------------------|-------------------|
| Pendiente | **Registrar Pago de Depósito**. |
| Pagado | **Devolver Depósito** o **Aplicar a Daño**. |

#### Incidentes

Si se reporta un incidente durante el uso del espacio:
1. Haga clic en **Reportar Incidente**.
2. Complete:
   - **Descripción** del incidente.
   - **Severidad**: Menor, Moderado, Grave, Crítico.
   - **Monto del Daño** (si aplica).
3. Guarde. El sistema cambia el estado de la reserva a **Con Incidente**.

### 15.6 Bandeja de administración

**Ruta:** `Reservas > Bandeja Admin`

Panel simplificado para acciones rápidas del administrador:

**Sección 1: Reservas Pendientes**
- Lista de reservas en estado Pendiente.
- Botones **Aprobar** y **Rechazar** directamente desde la bandeja.

**Sección 2: Reservas Aprobadas**
- Lista de reservas en estado Aprobada.
- Botón **Check-In** para iniciar el uso del espacio.

> Diseñado para flujo rápido: el administrador puede aprobar/rechazar y gestionar check-in sin entrar al detalle de cada reserva.

### 15.7 Calendario de reservas

**Ruta:** `Reservas > Calendario`

Vista mensual que muestra las reservas de un espacio seleccionado:

- **Selector de espacio**: elija el espacio a visualizar.
- **Navegación mensual**: botones anterior/siguiente.
- **Cuadrícula mensual**: cada día muestra las reservas como chips de color:
  - Amarillo: Pendiente.
  - Verde: Aprobada.
  - Azul: En Uso.
  - Gris: Completada.
  - Rojo: Cancelada/Rechazada.
  - Naranja: Con Incidente.
- Los chips son clickables para ir al detalle de la reserva.

### 15.8 Gestión de espacios reservables

**Ruta:** `Reservas > Espacios`

#### Lista de espacios

Muestra todos los espacios configurados en tarjetas con:

| Dato | Descripción |
|------|-------------|
| Nombre | Nombre del espacio. |
| Ubicación | Dónde se encuentra. |
| Estado | Activo o Inactivo. |
| Capacidad | Número máximo de personas. |
| Costo | Tipo de cobro (Por Hora, Por Evento, Sin costo). |
| Aprobación | Automática o Manual. |
| Política de Mora | Bloquear o Advertir. |
| Depósito | Monto del depósito si aplica. |

**Filtros:**
- **Búsqueda libre**: Por nombre o ubicación.
- **Estado**: Todos, Activos, Inactivos.

#### Crear un nuevo espacio

**Ruta:** `Reservas > Espacios > Nuevo Espacio`

1. Complete la **información general**:

| Campo | Descripción |
|-------|-------------|
| Nombre | Nombre del espacio (ej. "Salón Social"). |
| Descripción | Detalles del espacio (opcional). |
| Ubicación | Dónde se encuentra (opcional). |
| Capacidad Máxima | Número máximo de personas. |

2. Configure las **reglas de reserva**:

| Campo | Descripción |
|-------|-------------|
| Mínimo de Horas | Duración mínima de la reserva. |
| Máximo de Horas | Duración máxima de la reserva. |
| Anticipación Mínima | Horas mínimas antes de la reserva. |
| Anticipación Máxima | Días máximos de anticipación. |
| Reservas Simultáneas | Máximo de reservas activas por unidad. |

3. Configure **costos y depósito**:

| Campo | Descripción |
|-------|-------------|
| Cobro Adicional | Activar/desactivar cobro por el espacio. |
| Tipo de Cobro | Por Hora, Por Evento u Otro. |
| Tarifa | Valor según el tipo de cobro. |
| Requiere Depósito | Si se requiere depósito de garantía. |
| Monto del Depósito | Valor del depósito. |

4. Seleccione las **políticas**:

| Campo | Opciones |
|-------|----------|
| Modo de Aprobación | Automática (las reservas se aprueban solas) o Manual (requiere aprobación del admin). |
| Política de Mora | Bloquear (no permite reservar si hay mora) o Advertir (permite pero muestra advertencia). |

5. Haga clic en **Guardar Espacio**.

#### Detalle del espacio

**Ruta:** `Reservas > Espacios > [Espacio]`

Muestra toda la configuración del espacio y permite gestionar **horarios de disponibilidad**:

**Horarios por día de la semana:**
- Cada día (domingo a sábado) puede tener un horario configurado.
- Haga clic en **Agregar** para añadir un horario: seleccione el día, hora de inicio y hora de fin.
- Puede eliminar horarios existentes.

> Un espacio sin horarios configurados no estará disponible para reservas, aunque esté activo.

### 15.9 Notificaciones automáticas

El módulo de reservas genera las siguientes notificaciones automáticas:

| Evento | Descripción |
|--------|-------------|
| Reserva Aprobada | Notifica al residente que su reserva fue confirmada. |
| Reserva Rechazada | Notifica al residente que su solicitud fue rechazada (con motivo). |
| Recordatorio 24h | Recordatorio un día antes de la reserva. |
| Recordatorio 2h | Recordatorio dos horas antes de la reserva. |
| Depósito Devuelto | Confirmación de devolución del depósito de garantía. |


## 16. Roles y Permisos

El sistema cuenta con los siguientes roles:

| Rol | Descripción |
|-----|-------------|
| **SuperAdmin** | Acceso total a todos los módulos y conjuntos. |
| **Admin** | Administrador del conjunto. Acceso completo a configuración, finanzas y residentes. |
| **Council** | Miembro del Consejo de Administración. Puede aprobar traslados presupuestales y usos del fondo. |
| **Accountant** | Contador. Acceso a presupuesto, fondo de imprevistos y reportes. |
| **Auditor** | Acceso de solo lectura a reportes financieros. |
| **Resident** | Propietario o residente. Acceso solo a su unidad, su estado de cuenta y notificaciones. |

### 14.1 Permisos por módulo

| Módulo | Admin | Council | Accountant | Auditor | Resident |
|--------|-------|---------|------------|---------|----------|
| Dashboard | Completo | Resumen | Indicadores | Reportes | Solo su unidad |
| Unidades | CRUD | Lectura | Lectura | Lectura | Su unidad |
| Propietarios | CRUD | Lectura | Lectura | Lectura | — |
| Arrendatarios | CRUD | Lectura | Lectura | Lectura | — |
| Presupuesto | CRUD | Aprobar | CRUD | Lectura | — |
| Fondo Imprevistos | CRUD | Aprobar | CRUD | Lectura | — |
| Cuotas y Cartera | CRUD | — | CRUD | Lectura | Su estado de cuenta |
| Configuración | CRUD | — | — | — | — |
| PQR | CRUD | Responder, Alertas | Responder | Lectura | Radicar, Seguimiento |
| Proveedores | CRUD | Lectura | CRUD | Lectura | — |
| Contratos | CRUD | Aprobar, Alertas | CRUD | Lectura | — |
| Mantenimiento | CRUD | Lectura | Lectura | Lectura | — |
| Comunicaciones | CRUD | Lectura | Lectura | Lectura | Cartelera, Preferencias |
| Asambleas | CRUD | Participar, Votar | Lectura | Lectura | Participar, Votar |
| Reservas | CRUD | Lectura | Lectura | Lectura | Solicitar |

---

## 17. Preguntas Frecuentes

**¿Cómo recupero mi contraseña?**
Actualmente debe contactar al administrador del sistema para restablecerla.

**¿Por qué no puedo guardar una unidad?**
Verifique que la suma de coeficientes de todas las unidades activas sea exactamente 100.0000%. El sistema no permite guardar si hay diferencia.

**¿Cómo se calcula la cuota de administración?**
`Cuota = Presupuesto Mensual Total × (Coeficiente de la Unidad / 100)`.

**¿Qué pasa si una unidad no paga?**
El sistema calcula intereses de mora diariamente. Después de 30 días pasa a etapa prejurídica y después de 60 a jurídica.

**¿Cómo se actualiza el presupuesto después de activado?**
Solo mediante traslados o adiciones presupuestales, cada uno requiriendo aprobación del Consejo o Asamblea.

**¿Qué significa el estado "En Disputa" en un cobro individual?**
El propietario ha impugnado formalmente el cobro. Mientras esté en disputa, el cobro no genera intereses de mora.

**¿Puede un propietario tener varias unidades?**
Sí. Un propietario puede estar vinculado a múltiples unidades con diferentes porcentajes de propiedad.

**¿Cómo configuro la secuencia de avisos de mora?**
Vaya a Comunicaciones → Secuencia Mora. Allí puede definir los días después del vencimiento y la plantilla para cada uno de los 4 pasos (1er aviso, 2do aviso, 3er aviso, prejurídico).

**¿Un residente puede dejar de recibir comunicados?**
Puede solicitarlo al administrador, quien registrará la preferencia en Comunicaciones → Preferencias dejando constancia de la solicitud. Las notificaciones críticas (emergencias, cortes) siempre se envían.

**¿Qué pasa si un comunicado requiere confirmación de lectura y el residente no confirma?**
El administrador puede reenviar el comunicado solo a los no confirmantes desde la página de detalle del comunicado.

---

## 18. Glosario

| Término | Definición |
|---------|------------|
| **Coeficiente** | Porcentaje de participación de una unidad en las áreas comunes. Determina el valor de la cuota y los votos en asamblea. |
| **Cuota Ordinaria** | Cuota mensual de administración para gastos de funcionamiento. |
| **Cuota Extraordinaria** | Cuota adicional aprobada por asamblea para gastos no presupuestados. |
| **Cobro Individual** | Multa, daño o servicio adicional cobrado a una unidad específica. |
| **Paz y Salvo** | Certificado que acredita que una unidad no tiene deudas con la copropiedad. |
| **Unidad** | Cada propiedad privada (apartamento, casa, local) dentro del conjunto. |
| **Fondo de Imprevistos** | Reserva obligatoria (Ley 675 Art. 35) para gastos urgentes no presupuestados. |
| **Resolución 029** | Norma contable para propiedades horizontales en Colombia (2019). |
| **Inmutabilidad** | Principio contable: los registros financieros no se eliminan, solo se ajustan o revierten. |
| **Anticipo** | Excedente de un pago que se aplica al período siguiente. |
| **PQR** | Petición, Queja o Reclamo. Solicitud formal del residente a la administración. |
| **Radicado** | Número único de identificación de una PQR. Formato `PQR-YYYY-MM-NNNNN`. |
| **Escalada** | Estado de una PQR que superó su plazo de respuesta o fue elevada al Consejo de Administración. |
| **Proveedor** | Persona natural o jurídica que presta servicios o suministra bienes al conjunto. |
| **Contrato** | Acuerdo formal entre el conjunto y un proveedor para la prestación de servicios. |
| **Póliza de Seguro** | Documento que ampara riesgos asociados a un contrato (cumplimiento, responsabilidad civil). |
| **Retención en la Fuente** | Descuento que retiene el conjunto sobre los pagos a proveedores, conforme a la normativa tributaria. |
| **Retención ICA** | Retención del Impuesto de Industria, Comercio y Ocupación que aplica sobre pagos a proveedores. |
| **Nivel de Aprobación** | Autoridad requerida para aprobar un contrato según su valor (Administrador, Consejo, Asamblea). |
| **Evaluación de Proveedor** | Valoración periódica del desempeño de un proveedor en 4 criterios (calidad, cumplimiento, precio, post-venta). |
| **Bien Común** | Activo físico del conjunto destinado al uso y goce de todos los copropietarios (ascensores, bombas, piscinas, zonas verdes, etc.). |
| **Plan de Mantenimiento** | Definición periódica de actividades preventivas para conservar un bien común en óptimas condiciones. |
| **Orden de Trabajo** | Registro formal de una intervención de mantenimiento (preventivo o correctivo) sobre un bien común. |
| **Siniestro** | Evento extraordinario (inundación, incendio, daño estructural) que afecta bienes comunes del conjunto. |
| **Fuera de Servicio** | Estado de un bien que impide su uso temporal o definitivamente, bloqueando reservas. |
| **Evidencia Fotográfica** | Registro visual del estado de un bien antes y después de una intervención de mantenimiento. |
| **Comunicado** | Documento formal de la administración con membrete, asunto y cuerpo, que queda registrado con seguimiento de entrega. |
| **Notificación Automática** | Mensaje corto generado automáticamente por un evento de otro módulo (pago, PQR, reserva, etc.). |
| **Confirmación de Lectura** | Mecanismo que requiere que el destinatario marque explícitamente que leyó un comunicado. |
| **Cartelera Digital** | Mural de publicaciones visible para todos los residentes autenticados en el portal. |
| **Secuencia de Mora** | Progresión de 4 avisos automáticos (1, 5, 15, 30 días) para unidades en mora. |
| **Plantilla de Notificación** | Texto configurable con variables dinámicas que se reemplazan automáticamente al generar una notificación. |

---

> **Nota:** Este manual se actualizará a medida que se agreguen nuevos módulos y funcionalidades al sistema. Para sugerencias o reportes de errores, contacte al equipo de desarrollo.

---
*Documento generado el Junio 2026 — Softcoinp ERP*

---

## 19. Módulo de Reportes y Exportaciones

Este módulo centraliza todos los reportes del sistema. Podrá generar informes financieros, de cartera, operativos, de asamblea y anuales, consultar el historial de reportes generados, configurar reportes recurrentes, construir el informe anual de gestión, personalizar la apariencia de los PDF y controlar el acceso según su rol.

### 17.1 Catálogo de Reportes (página /reports)

Acceda desde el menú lateral: **Reportes → Catálogo**.

Aquí encontrará la lista completa de todos los tipos de reporte disponibles en el sistema. La página se adapta según su rol: solo verá los reportes que puede generar.

**La pantalla muestra:**
- **Filtros por categoría**: Todos, Financieros, Cartera, Operativos, Asamblea, Anuales.
- **Barra de búsqueda**: Busque por nombre o descripción del reporte.
- **Tarjetas de reporte**: Cada reporte muestra su nombre, una descripción breve, uno o dos badges de formato (PDF, Excel) y un badge adicional de color rojo si el reporte **contiene datos personales** (marcado como "Datos Personales").
- **Botón "Generar"**: Abre un modal para configurar la generación del reporte.

**Modal de generación:**
1. Haga clic en **Generar** en el reporte deseado.
2. Seleccione el **período**:
   - **Desde** (fecha de inicio).
   - **Hasta** (fecha de fin).
3. Seleccione el **Formato**: PDF o Excel.
4. Opcional: agregue **Notas** que aparecerán en el reporte.
5. Haga clic en **Generar**. El reporte pasará a la cola de generación y aparecerá en el historial cuando esté listo.

> Los reportes con datos personales solo pueden ser generados por usuarios con permisos suficientes. Al generarlos, el sistema aplica las restricciones de la Ley 1581 de 2012.

#### Reportes disponibles por categoría

**Financieros (7 reportes):**

| Reporte | Descripción | Formatos |
|---------|-------------|----------|
| Balance General | Activos, pasivos y patrimonio a una fecha determinada. | PDF, Excel |
| Estado de Resultados | Ingresos y egresos del período seleccionado. | PDF, Excel |
| Balance de Comprobación | Listado de cuentas con movimientos débito/crédito y saldos. | PDF, Excel |
| Mayor Contable | Historial detallado de movimientos de una cuenta específica con saldo corrido. | PDF, Excel |

| Ejecución Presupuestal | Comparativo entre presupuesto ajustado y ejecutado por rubro. | PDF, Excel |
| Detalle de Adiciones y Traslados | Movimientos presupuestales (adiciones y traslados) del período. | PDF, Excel |

**Cartera (6 reportes):**

| Reporte | Descripción | Formatos |
|---------|-------------|----------|
| Cartera por Unidad | Saldo pendiente de cada unidad, incluye días de mora y etapa de cobro. | PDF, Excel |
| Cartera por Etapa | Desglose de cartera agrupada por etapa: Preventivo, Prejurídico, Jurídico. | PDF, Excel |
| Estado de Cuenta Individual | Movimientos y saldo de una unidad específica. | PDF |
| Intereses de Mora | Cálculo detallado de intereses generados por unidad. | PDF, Excel |
| Acuerdos de Pago | Listado de acuerdos de pago activos e históricos. | PDF, Excel |
| Paz y Salvo | Certificado de paz y salvo por unidad. | PDF |

> Los reportes "Estado de Cuenta Individual" y "Paz y Salvo" contienen datos personales y están restringidos según el rol del usuario.

**Operativos (5 reportes):**

| Reporte | Descripción | Formatos |
|---------|-------------|----------|
| Padrón de Propietarios | Listado completo de propietarios con datos de contacto y unidades vinculadas. | PDF, Excel |
| Padrón de Arrendatarios | Listado de arrendatarios activos con datos de contacto y vigencia del contrato. | PDF, Excel |
| Inventario de Bienes Comunes | Listado de activos físicos del conjunto con estado y ubicación. | PDF, Excel |
| Órdenes de Trabajo | Historial de órdenes de trabajo del período seleccionado. | PDF, Excel |
| Proveedores y Contratos | Listado de proveedores y contratos activos. | PDF, Excel |

> Los reportes "Padrón de Propietarios" y "Padrón de Arrendatarios" contienen datos personales.

**Asamblea (4 reportes):**

| Reporte | Descripción | Formatos |
|---------|-------------|----------|
| Convocatoria a Asamblea | Documento formal de convocatoria con orden del día. | PDF |
| Acta de Asamblea | Acta generada a partir de la información registrada en el sistema. | PDF |
| Lista de Asistentes | Registro de asistentes a la asamblea con unidades y coeficientes. | PDF, Excel |
| Certificación de Deuda | Certificado de deuda para ejercicio del derecho al voto. | PDF |

> El reporte "Lista de Asistentes" contiene datos personales.

**Anuales (4 reportes):**

| Reporte | Descripción | Formatos |
|---------|-------------|----------|
| Informe de Gestión del Consejo | Informe anual de actividades del Consejo de Administración. | PDF |
| Informe Financiero Anual | Resumen financiero del año fiscal con indicadores clave. | PDF |
| Informe de Recaudo Anual | Comparativo mensual de recaudo vs. facturado del año. | PDF, Excel |
| Informe de Morosidad | Análisis anual de morosidad con tendencias. | PDF, Excel |

### 17.2 Historial de Reportes (página /reports/history)

Acceda desde el menú lateral: **Reportes → Historial**.

Aquí encontrará todos los reportes que han sido generados en el sistema, ordenados del más reciente al más antiguo.

**La tabla muestra las siguientes columnas:**

| Columna | Descripción |
|---------|-------------|
| Reporte | Nombre del tipo de reporte generado. |
| Formato | PDF o Excel. |
| Período | Rango de fechas seleccionado al generar. |
| Generado por | Usuario que solicitó la generación. |
| Fecha | Fecha y hora de generación. |
| Tamaño | Tamaño del archivo generado. |

**Filtros disponibles:**
- **Tipo de reporte**: Seleccione un reporte específico para filtrar.
- **Rango de fechas**: Desde / Hasta para filtrar por fecha de generación.

**Acciones:**
- **Descargar**: Haga clic en el botón **Descargar** de la fila correspondiente para abrir o guardar el archivo generado.

> Si no hay reportes generados, la página mostrará el mensaje: *"Aún no se han generado reportes. Vaya al catálogo de reportes para generar su primer reporte."*

### 17.3 Reportes Recurrentes (página /reports/recurring)

Acceda desde el menú lateral: **Reportes → Recurrentes**.

Esta sección le permite configurar reportes que se generan automáticamente según una frecuencia definida.

#### Crear una nueva configuración recurrente

1. Haga clic en **Nueva Configuración**.
2. Complete los campos del modal:

| Campo | Descripción |
|-------|-------------|
| **Tipo de Reporte** | Seleccione el reporte a generar automáticamente. |
| **Nombre** | Asigne un nombre descriptivo a esta configuración. |
| **Frecuencia** | Diario, Semanal, Mensual, Trimestral o Anual. |
| **Formato** | PDF o Excel. |
| **Correos Destinatarios** | Uno o más correos electrónicos separados por coma. |
| **Asunto del Correo** | Asunto personalizado para el envío. |

3. Haga clic en **Guardar**.

**Lista de configuraciones:**

Cada configuración en la lista muestra:
- **Nombre** de la configuración.
- **Tipo de reporte** y **Formato**.
- **Frecuencia** programada.
- **Estado**: Activo (generándose normalmente), Pausado (temporalmente detenido) o Completado (configuración finalizada).
- **Próxima ejecución**: Fecha y hora estimada de la próxima generación automática.
- **Destinatarios**: Correos configurados.

**Botones por configuración:**
- **Pausar**: Detiene temporalmente la generación automática. El estado cambia a "Pausado".
- **Reanudar**: Reactiva la generación automática. El estado vuelve a "Activo".

> El motor de recurrencia ejecuta cada **5 minutos** para verificar si hay configuraciones que deban generar un reporte en ese momento. Si encuentra una configuración cuya próxima ejecución ya venció, la procesa inmediatamente.

### 17.4 Informe Anual de Gestión (página /reports/annual)

Acceda desde el menú lateral: **Reportes → Informe Anual**.

Este módulo le permite construir el **Informe Anual de Gestión del Consejo de Administración** de forma incremental. El informe se compone de varias secciones que puede auto-generar, revisar, editar manualmente y finalmente consolidar en un PDF.

#### Flujo de trabajo

El proceso recomendado es:

1. **Auto-generar secciones**: Use el botón **Regenerar** en cada sección para que el sistema la complete con datos reales del período.
2. **Revisar y editar**: Modifique el título y el contenido de cada sección según sea necesario.
3. **Consolidar**: Una vez todas las secciones estén listas, haga clic en **Consolidar Informe** para generar el PDF final.

#### Barra de progreso

En la parte superior de la página verá una barra de progreso que muestra el **porcentaje de completitud** del informe. Cada sección completada (en estado "Auto-generado" o "Editado manualmente") incrementa el progreso. Las secciones en "Pendiente" no cuentan.

#### Lista de secciones

Cada sección del informe se muestra como un panel expandible con la siguiente información:

- **Título de la sección** (editable).
- **Estado**: Uno de tres estados:
  - **Auto-generado**: El sistema completó la sección con datos del sistema. Puede revisarla y editarla.
  - **Editado manualmente**: Usted modificó el contenido después de la auto-generación, o escribió el contenido desde cero.
  - **Pendiente**: La sección aún no ha sido generada ni editada.

**Acciones por sección:**
- **Expandir/Colapsar**: Haga clic en la sección para ver su contenido.
- **Editar título**: Haga clic sobre el título y escríbalo directamente.
- **Editar contenido**: En el área de texto de la sección, modifique el contenido según necesite.
- **Regenerar Sección**: Disponible solo para secciones auto-generables. El sistema vuelve a generar el contenido con los datos más actualizados del período.

> Si edita manualmente una sección que previamente fue auto-generada, el estado cambia a "Editado manualmente". Si desea volver a la versión auto-generada, use el botón **Regenerar Sección**.

#### Consolidar Informe

Cuando todas las secciones estén completas:

1. Haga clic en **Consolidar Informe**.
2. Seleccione el **año fiscal** del informe.
3. El sistema genera un PDF profesional con todas las secciones, numeración automática y portada.
4. El PDF se guarda en el historial de reportes y puede descargarse desde allí.

> Puede consolidar el informe aunque haya secciones en estado "Pendiente", pero el sistema le mostrará una advertencia antes de proceder.

### 17.5 Plantillas PDF (página /reports/templates)

Acceda desde el menú lateral: **Reportes → Plantillas PDF**.

Esta página le permite personalizar la apariencia de los reportes que se generan en formato PDF. Cada tipo de reporte que soporta PDF tiene su propia plantilla.

#### Lista de plantillas

La página muestra una lista con una entrada por cada tipo de reporte que genera PDF. Cada plantilla se puede editar de forma independiente.

#### Personalización disponible

Al hacer clic en una plantilla, se despliegan los siguientes campos editables:

| Campo | Descripción |
|-------|-------------|
| **Texto de Encabezado** | Texto que aparece en la parte superior de cada página del reporte. |
| **Texto de Pie de Página** | Texto que aparece en la parte inferior de cada página. |
| **Nombre del Firmante** | Nombre de la persona que firma el reporte (ej. el administrador). |
| **Cargo del Firmante** | Cargo del firmante (ej. Administrador, Presidente del Consejo). |
| **Color Primario** | Selector de color para los títulos y bordes del reporte. |
| **Color Secundario** | Selector de color para las tablas y detalles decorativos. |
| **Nota de Confidencialidad** | Texto legal de confidencialidad. Se muestra solo en reportes marcados con "Datos Personales". |
| **Nota de Descargo** | Texto de descargo de responsabilidad. Se muestra solo en reportes financieros. |
| **Logo** | Subida de imagen (PNG o SVG) para el membrete del reporte. |

#### Cómo personalizar una plantilla

1. Busque el reporte en la lista y haga clic sobre él.
2. Edite los campos que desee modificar.
3. Los cambios se guardan automáticamente al salir del campo.

> Los cambios aplican **inmediatamente** a todos los nuevos reportes que se generen. Los reportes ya generados no se ven afectados.

### 17.6 Roles y Permisos

El acceso al módulo de Reportes y Exportaciones está controlado por rol, tanto para la generación de reportes como para la visualización de datos personales.

| Rol | Acceso |
|-----|--------|
| **SuperAdmin** | Acceso completo a los 26 reportes, historial, reportes recurrentes, informe anual y plantillas PDF. Sin restricciones de datos personales. |
| **Admin** | Acceso completo a los 26 reportes, historial, reportes recurrentes, informe anual y plantillas PDF. Sin restricciones de datos personales. |
| **Council** | Acceso a reportes financieros (7), operativos (5), asamblea (4) y anuales (4). **No puede generar** reportes que contengan datos personales (Estado de Cuenta Individual, Paz y Salvo, Padrón de Propietarios, Padrón de Arrendatarios, Lista de Asistentes). |
| **Accountant** | Acceso a reportes financieros (7), cartera seleccionada (Cartera por Unidad, Cartera por Etapa, Intereses de Mora, Acuerdos de Pago) y padrón de propietarios. No puede ver Estado de Cuenta Individual ni Paz y Salvo de unidades que no administra. |
| **Auditor** | Acceso de solo lectura a reportes financieros, cartera seleccionada y padrón de propietarios. Puede descargar reportes generados por otros usuarios, pero no generar nuevos reportes. |
| **Resident** | Solo puede generar su propio **Estado de Cuenta Individual** (cartera por unidad). No tiene acceso al catálogo completo, historial, reportes recurrentes, informe anual ni plantillas PDF. |

> **Nota sobre datos personales:** Los reportes que incluyen información de personas naturales (propietarios, arrendatarios, residentes) están sujetos a la Ley 1581 de 2012. El sistema restringe su generación según el rol y registra en el historial de auditoría cada vez que se genera uno de estos reportes.

---

*Documento generado el Junio 2026 — Softcoinp ERP*
