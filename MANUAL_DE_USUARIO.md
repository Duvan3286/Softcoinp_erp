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
6. [Módulo de Contabilidad](#6-módulo-de-contabilidad)
7. [Módulo de Presupuesto](#7-módulo-de-presupuesto)
8. [Módulo de Fondo de Imprevistos](#8-módulo-de-fondo-de-imprevistos)
9. [Módulo de Cuotas y Cartera](#9-módulo-de-cuotas-y-cartera)
10. [Módulo de Configuración](#10-módulo-de-configuración)
11. [Módulo PQR](#11-módulo-pqr)
12. [Módulo de Proveedores y Contratos](#12-módulo-de-proveedores-y-contratos)
13. [Módulo de Mantenimiento y Zonas Comunes](#13-módulo-de-mantenimiento-y-zonas-comunes)
14. [Roles y Permisos](#14-roles-y-permisos)
15. [Preguntas Frecuentes](#15-preguntas-frecuentes)
16. [Glosario](#16-glosario)

---

## 1. Introducción

**Softcoinp ERP** es un sistema de planificación de recursos empresariales diseñado para la administración de propiedades horizontales (conjuntos residenciales, edificios de apartamentos, centros comerciales) en Colombia.

### 1.1 ¿Qué puede hacer con este sistema?

- Administrar el catálogo de unidades (apartamentos, casas, locales)
- Gestionar propietarios, arrendatarios y grupos de convivencia
- Liquidar cuotas de administración ordinarias y extraordinarias
- Registrar pagos y administrar la cartera
- Gestionar el plan de cuentas contable (Resolución 029 de 2019)
- Registrar asientos contables y generar reportes financieros
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

## 6. Módulo de Contabilidad

### 6.1 Plan de Cuentas

Acceda desde el menú: **Finanzas → Plan de Cuentas**.

El plan de cuentas sigue la **Resolución 029 de 2019** del Consejo Técnico de la Contaduría Pública, adaptada para propiedades horizontales.

**Estructura jerárquica:**

| Nivel | Longitud | Ejemplo |
|-------|----------|---------|
| Clase | 1 dígito | `1` Activo |
| Grupo | 2 dígitos | `11` |
| Cuenta | 4 dígitos | `1105` Caja |
| Subcuenta | 6 dígitos | `110501` Caja General |
| Auxiliar | 8 dígitos | `11050101` |

**Actions disponibles:**
- **Filtrar** por categoría (Activo, Pasivo, Ingreso, Gasto) o naturaleza (Débito, Crédito).
- **Crear cuenta auxiliar** bajo una cuenta existente (solo cuentas no oficiales).
- **Editar nombre** o estado de cuentas auxiliares.
- **Eliminar** cuentas auxiliares creadas por el usuario.

> Las cuentas del estándar oficial **no pueden modificarse ni eliminarse**.

### 6.2 Períodos Contables

Acceda desde el menú: **Finanzas → Períodos Contables**.

Cada mes calendario debe tener un período contable abierto para registrar asientos.

**Pasos:**
1. **Abrir período**: Seleccione año y mes. El sistema crea el período en estado `Abierto`.
2. **Cerrar período**: Al finalizar el mes, puede cerrarlo. Un período cerrado no acepta nuevos asientos.

### 6.3 Libro Diario (Asientos Contables)

Acceda desde el menú: **Finanzas → Libro Diario**.

#### Crear un asiento contable

1. Haga clic en **Nuevo Asiento**.
2. Seleccione la **fecha del asiento**.
3. Ingrese una **descripción** del movimiento.
4. Agregue **líneas de asiento** (mínimo 2, una de débito y una de crédito):

   - Seleccione la cuenta contable.
   - Ingrese el valor en débito o crédito (nunca ambos en la misma línea).

5. Verifique que la suma de débitos sea igual a la suma de créditos.
6. Guarde como **Borrador** o **Contabilice** directamente.

#### Contabilizar un asiento

Un asiento en borrador puede editarse. Al **contabilizarlo**:
- Pasa a estado `Final` (inmutable).
- Se asigna un número correlativo único.
- Se contabiliza en el período correspondiente.

#### Revertir un asiento

Para corregir un asiento contabilizado:
1. Abra el detalle del asiento.
2. Haga clic en **Revertir**.
3. Ingrese el **motivo** de la reversión.
4. El sistema genera automáticamente un nuevo asiento con los valores opuestos.

### 6.4 Reportes Contables

Acceda desde el menú: **Finanzas → Reportes Contables**.

Cuatro reportes disponibles:

| Reporte | Descripción |
|---------|-------------|
| **Balance de Comprobación** | Listado de todas las cuentas con sus movimientos débito/crédito y saldos. |
| **Mayor Contable** | Historial detallado de movimientos de una cuenta específica con saldo corrido. |
| **Estado de Resultados** | Ingresos y egresos del período seleccionado (PyG). |
| **Balance General** | Activos, pasivos y patrimonio a una fecha determinada. |

Filtre por **período contable** para ver los reportes del mes deseado.

---

## 7. Módulo de Presupuesto

### 7.1 Presupuesto Anual

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

### 7.2 Vista de ejecución

El presupuesto activo muestra la ejecución en tiempo real:

- **Presupuesto Inicial**: Valor aprobado por la asamblea.
- **Adiciones**: Incrementos aprobados durante el año.
- **Traslados**: Movimientos entre rubros.
- **Presupuesto Ajustado**: Inicial + Adiciones ± Traslados.
- **Ejecutado**: Gastos reales registrados en contabilidad.
- **Disponible**: Presupuesto ajustado - Ejecutado.
- **% Ejecutado**: Porcentaje de ejecución.
- **Proyección**: Estimación a fin de año basada en la tendencia actual.

> El sistema muestra **alertas** cuando un rubro supera el 90% de ejecución o cuando la proyección supera el presupuesto ajustado.

---

## 8. Módulo de Fondo de Imprevistos

### 8.1 Fondo de Imprevistos (Ley 675)

Acceda desde el menú: **Finanzas → Fondo Imprevistos**.

El **Artículo 35 de la Ley 675 de 2001** establece que toda copropiedad debe constituir un fondo de imprevistos con un mínimo del **1% de los ingresos** del período.

### 8.2 Liquidar aporte mensual

1. Seleccione el **año** y **mes** a liquidar.
2. Haga clic en **Liquidar Aporte Mensual**.
3. El sistema calcula automáticamente:

   - **Base de ingresos**: Suma de ingresos del período.
   - **Porcentaje aplicado**: El configurado en el conjunto (mínimo 1%).
   - **Monto del aporte**: Base × Porcentaje / 100.

4. El sistema genera automáticamente el asiento contable (Débito Gasto / Crédito Fondo Social).

> **Tope de acumulación**: Si el saldo del fondo supera el **10% del presupuesto anual**, el sistema no generará el aporte para evitar acumulación excesiva.

### 8.3 Registrar uso del fondo

1. Haga clic en **Registrar Uso del Fondo**.
2. Complete:

   - **Monto** a retirar (no puede superar el saldo disponible).
   - **Justificación** del gasto imprevisto.
   - **Acta del Consejo de Administración** que aprobó el retiro.
   - **Fecha de aprobación**.

3. Guarde. El sistema genera el asiento contable correspondiente.

---

## 9. Módulo de Cuotas y Cartera

### 9.1 Períodos de Liquidación

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

### 9.2 Cuotas Extraordinarias

Acceda desde el menú: **Finanzas → Facturación → Cuotas Extraordinarias** (pestaña).

1. Haga clic en **Nueva Cuota Extraordinaria**.
2. Complete:

   - **Nombre**: Ej. "Impermeabilización Fachada 2026".
   - **Monto total** aprobado por la asamblea.
   - **Número de cuotas** (contados).
   - **Tipo de distribución**: Por coeficiente o grupo específico.
   - **Período de inicio**.

3. Guarde. El sistema genera automáticamente la distribución por unidad.

### 9.3 Cobros Individuales

Acceda desde el menú: **Finanzas → Facturación → Cobros Individuales** (pestaña).

Útil para:
- **Multas** (Art. 58 Ley 675)
- **Daños a bienes comunes**
- **Parqueaderos visitantes**
- **Otros cobros particulares**

1. Haga clic en **Nuevo Cobro**.
2. Seleccione la **unidad**, **tipo de cobro**, **concepto** y **monto**.
3. Si aplica, indique si está **en disputa**.

### 9.4 Cartera

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

### 9.5 Acuerdos de Pago

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

### 9.6 Registro de Pagos

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

### 9.7 Estados de Cuenta y Paz y Salvos

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

## 10. Módulo de Configuración

Acceda desde el menú inferior: **Configuración**.

### 10.1 Legal e Identidad

- **Nombre oficial del conjunto** y NIT (con DV automático).
- **Dirección**, municipio, departamento.
- **Datos del representante legal** (nombre, documento).
- **Logo del conjunto** (formato PNG o SVG).

### 10.2 Financiero

| Parámetro | Descripción |
|-----------|-------------|
| Día de Corte | Día del mes para cierre de facturación. |
| Días de Gracia | Días adicionales después del vencimiento sin interés. |
| Tasa de Interés Máxima Legal | Tasa máxima permitida por ley. |
| Tasa de Interés Aplicada | Tasa que cobra el conjunto (no puede exceder la máxima legal). |
| Inicio Año Fiscal | Mes en que comienza el año contable. |

### 10.3 Operativo

- **Total de unidades y torres** del conjunto.
- **Política de redondeo** para liquidación de cuotas.
- **Máximo de cuotas extraordinarias activas** simultáneas.
- **Porcentaje del fondo de imprevistos** (mínimo 1%).

### 10.4 Notificaciones

- **Correo remitente** para notificaciones automáticas.
- **Frecuencia de notificaciones** a morosos.
- **Plantilla de pie de firma** para comunicaciones.

### 10.5 Documentos

Gestión de documentos oficiales:

- Cargue documentos por rol (Administración, Consejo, Auditor).
- Formatos soportados: PDF.
- Descargue documentos previamente cargados.

### 10.6 Historial de Auditoría

Registro de todos los cambios realizados en la configuración:

- Parámetro modificado.
- Valor anterior y nuevo valor.
- Usuario que realizó el cambio.
- Fecha y hora.

---

## 11. Módulo PQR (Peticiones, Quejas y Reclamos)

### 11.1 ¿Qué es una PQR?

**PQR** significa **Petición, Queja o Reclamo**. Es el canal oficial de comunicación entre los residentes y la administración del conjunto. La Ley 675 de 2001 establece la obligación del administrador de atender y responder las solicitudes de los copropietarios dentro de plazos razonables.

| Tipo | Descripción | Plazo recomendado |
|------|-------------|-------------------|
| **Petición** | Solicitud de información, servicio o acción específica (ej. solicitar paz y salvo, pedir uso del salón comunal). | 5 días hábiles |
| **Queja** | Manifestación de inconformidad por conductas que afectan la convivencia (ej. ruido excesivo, mal uso de zonas comunes). | 3 días hábiles |
| **Reclamo** | Inconformidad relacionada con cobros, sanciones o decisiones administrativas (ej. cobro incorrecto en la cuota). | 10 días hábiles |

> Los plazos pueden ser ajustados por el administrador según el reglamento del conjunto.

### 11.2 Radicar una PQR

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

### 11.3 Bandeja del Administrador

Acceda a la lista de PQR activas ordenadas por urgencia. Cada PQR muestra:

- **Semáforo de tiempo**: Verde (dentro del plazo), Amarillo (50-80%), Rojo (más de 80% o vencida).
- **Indicador de prioridad**: Alta, Media o Baja.
- **Número de radicado**, tipo, estado y unidad.

Puede filtrar por:
- Estado (Radicada, En Revisión, En Trámite, Respondida, Cerrada, Reabierta, Escalada)
- Tipo (Petición, Queja, Reclamo)
- Internas (mostrar/ocultar)

### 11.4 Detalle de una PQR

Al abrir una PQR, encuentra:

- **Información general**: Datos del radicante, unidad, tipo, categoría, canal.
- **Historial de estados**: Todos los cambios de estado con fecha, usuario y justificación.
- **Respuestas**: Cada respuesta emitida por la administración, con opción de marcar como definitiva o parcial.
- **Notas internas** (solo visible para Administración/Consejo/Contador): Notas del equipo que no son visibles para el residente.
- **Archivos adjuntos**: Documentos subidos por el radicante y por la administración.
- **Alertas**: Alertas generadas por vencimiento de tiempos.

### 11.5 Responder una PQR

1. Desde el detalle de la PQR, haga clic en **Responder**.
2. Redacte el texto de la respuesta.
3. Adjunte archivos de soporte si es necesario.
4. Seleccione si la respuesta es **Definitiva** (cierra la PQR) o **Parcial** (actualización de estado).
5. Si requiere que el residente confirme haber recibido la respuesta, active la opción.
6. Haga clic en **Enviar Respuesta**.

### 11.6 Cambiar estado de una PQR

| Acción | Nuevo Estado | Cuándo usarlo |
|--------|-------------|---------------|
| Revisar | En Revisión | Cuando el administrador abre la PQR para analizarla. |
| Asignar | En Trámite | Cuando se asigna a un responsable interno. |
| Responder | Respondida | Cuando se emite una respuesta formal. |
| Cerrar | Cerrada | Cuando el radicante confirmó o venció el plazo de confirmación. |
| Reabrir | Reabierta | Cuando el radicante considera insatisfactoria la respuesta (dentro de 10 días). |
| Escalar | Escalada | Cuando se requiere intervención del Consejo de Administración. |

### 11.7 Alertas automáticas

El sistema monitorea automáticamente los tiempos de respuesta:

| Umbral | Acción |
|--------|--------|
| **50% del plazo** | Se genera una alerta interna para el administrador si la PQR sigue en estado Radicada o En Revisión. |
| **80% del plazo** | Se genera una alerta y se escala al Consejo de Administración. |
| **100% (vencimiento)** | La PQR se marca automáticamente como **Escalada** y se genera una alerta crítica en el Dashboard. |

Las alertas activas se pueden consultar desde el panel de indicadores y resolver manualmente cuando la situación esté controlada.

### 11.8 Vínculo con cartera (reclamos de cobro)

Si un reclamo está relacionado con un cobro (cuota ordinaria, extraordinaria o cobro individual):

1. Al radicar, marque la opción **Vinculado a Cobro** y seleccione el cobro correspondiente.
2. El sistema vincula el reclamo al estado de cuenta de la unidad.
3. Cuando el administrador o el consejo resuelva el reclamo:
   - **Procedente**: Marque "Reclamo Resuelto = Sí". El sistema genera automáticamente una **nota de crédito** en el módulo de cuotas, ajustando el saldo sin intervención manual adicional.
   - **Improcedente**: Marque "Reclamo Resuelto = No". El cobro se mantiene y el radicante es notificado.

### 11.9 Configuración de tiempos

El administrador puede ajustar los plazos de respuesta desde la configuración del módulo:

1. Acceda a **Configuración de Tiempos PQR**.
2. Para cada tipo (Petición, Queja, Reclamo), defina los **días hábiles** de respuesta.
3. Guarde los cambios.

> Los valores por defecto son: Petición 5 días, Queja 3 días, Reclamo 10 días.

### 11.10 Panel de indicadores PQR

El administrador cuenta con un tablero de indicadores que muestra:

- **Total de PQR**: Abiertas, cerradas y escaladas.
- **Alertas activas**: Alertas pendientes por vencimiento.
- **Tiempo promedio de respuesta**: En horas, por tipo de PQR.
- **Distribución por tipo y categoría**: Gráfico de torta.
- **Tendencia mensual**: Número de radicaciones por mes.
- **Estado actual**: Cantidad de PQR en cada estado.

### 11.11 Portal del residente

Los residentes pueden:

- Ver el listado de sus PQR activas e históricas (solo las no internas).
- Consultar el detalle de cada PQR con su historial de estados y respuestas.
- **No pueden ver las notas internas** del equipo de administración.

---

## 12. Módulo de Proveedores y Contratos

Gestión integral de proveedores, contratos, facturas, pagos, evaluaciones de desempeño y configuración de retenciones.

### 12.1 Bandeja de Proveedores

**Ruta:** `Proveedores > Proveedores`

La bandeja muestra todos los proveedores registrados con filtros por estado, tipo y búsqueda por nombre o documento.

**Acciones disponibles:**
- **Nuevo Proveedor**: Crea un nuevo proveedor con toda su información.
- **Ver**: Accede al detalle del proveedor con sus contratos y evaluaciones.

**Filtros:**
- **Estado**: Todos, Activos, Inactivos.
- **Tipo**: Todos, Natural, Jurídica.
- **Búsqueda libre**: Por nombre, documento o contacto.

### 12.2 Crear Proveedor

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

### 12.3 Detalle del Proveedor

**Ruta:** `Proveedores > [Proveedor]`

Muestra toda la información del proveedor organizada en secciones:

- **Información del Proveedor**: Datos básicos, tipo, documento, actividad económica.
- **Contacto**: Email, teléfono, dirección, ciudad.
- **Representante Legal**: Solo si el tipo es Jurídica.
- **Contratos**: Lista de contratos asociados con valor, fechas y estado. Botón "Nuevo Contrato" para crear uno vinculado.
- **Evaluaciones**: Historial de evaluaciones con puntaje promedio y recomendación. Botón "Evaluar" para crear una nueva evaluación con scoring del 1-5 en 4 criterios.

### 12.4 Bandeja de Contratos

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

### 12.5 Crear Contrato

**Ruta:** `Proveedores > Nuevo Contrato`

| Sección | Campos Obligatorios | Descripción |
|---------|---------------------|-------------|
| **Información del Contrato** | Proveedor, Nro. Contrato, Tipo, Objeto | Seleccione el proveedor, defina el tipo y describa el objeto del contrato. |
| **Vigencia y Valores** | Valor Total, Fecha Inicio, Fecha Fin | Defina el valor, las fechas de vigencia y si es recurrente o tiene renovación automática. |

**Reglas:**
- El nivel de aprobación se determina automáticamente según los umbrales configurados.
- Para contratos con aprobación de Consejo o Asamblea, se requiere el número de acta al activar.
- Solo los contratos en estado Borrador pueden editarse o eliminarse.

### 12.6 Detalle del Contrato

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

### 12.7 Indicadores de Proveedores

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

### 12.8 Motor de Alertas de Contratos

El sistema ejecuta un servicio en segundo plano cada 6 horas que genera automáticamente:

| Tipo de Alerta | Condición | Escalada al Consejo |
|----------------|-----------|---------------------|
| Vencimiento 90 días | Contrato activo con ≤90 días para vencer | No |
| Vencimiento 30 días | Contrato activo con ≤30 días para vencer | No |
| Vencimiento 15 días | Contrato activo con ≤15 días para vencer | Sí |
| Renovación Automática | Contrato con renovación automática a punto de vencer | No |
| Póliza por Vencer | Póliza activa con ≤30 días para vencer | No |

Las alertas se pueden resolver manualmente desde el detalle del contrato. Las alertas resueltas con más de 30 días se limpian automáticamente.

### 12.9 Configuración de Retenciones

**Ruta:** `Proveedores > Contratos > Configuración de Retenciones`

Permite configurar las tarifas de retención por tipo de servicio:

| Campo | Descripción |
|-------|-------------|
| **Tipo de Servicio** | Categoría del servicio (Mantenimiento, Aseo, etc.) |
| **Tarifa Retención Fuente** | Porcentaje de retención en la fuente (ej. 2.5%) |
| **Tarifa Retención ICA** | Porcentaje de retención ICA (ej. 0.28%) |

### 12.10 Umbrales de Aprobación

**Ruta:** `Proveedores > Contratos > Umbrales de Aprobación`

Configura los rangos de valor para determinar qué nivel aprueba un contrato:

| Nivel | Descripción |
|-------|-------------|
| **Administrador** | Contratos menores al umbral mínimo del Consejo |
| **Consejo** | Contratos dentro del rango del Consejo de Administración |
| **Asamblea** | Contratos superiores al umbral del Consejo |

Si el valor del contrato no cae en ningún rango configurado, el nivel por defecto es Administrador.

---

## 13. Módulo de Mantenimiento y Zonas Comunes

Gestión integral del inventario físico de bienes comunes, planes de mantenimiento preventivo, órdenes de trabajo correctivo y registro de siniestros. Este módulo protege el patrimonio colectivo de los copropietarios y garantiza que las zonas comunes se conserven en condiciones óptimas.

### 13.1 Inventario de Bienes Comunes

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

### 13.2 Registrar Nuevo Bien Común

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

### 13.3 Detalle del Bien Común

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

### 13.4 Planes de Mantenimiento Preventivo

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

### 13.5 Órdenes de Trabajo

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

### 13.6 Registrar Orden de Trabajo

**Ruta:** `Mantenimiento > Nueva Orden de Trabajo`

| Sección | Campos Obligatorios | Descripción |
|---------|---------------------|-------------|
| **Información de la Orden** | Tipo, Bien, Descripción, Prioridad | Seleccione si es preventiva o correctiva, el bien afectado y describa el trabajo. |
| **Asignación** | (Ninguno obligatorio) | Proveedor asignado, fecha programada de ejecución. |
| **Costos y Contabilidad** | (Ninguno obligatorio) | Costo estimado, cuenta presupuestal del PUC a imputar. |

**Reglas:**
- Si la orden es correctiva originada desde una PQR, seleccione la PQR en el campo correspondiente.
- Las órdenes de emergencia aplican solo a bienes esenciales.
- El sistema genera automáticamente órdenes preventivas según los planes configurados.

### 13.7 Detalle de Orden de Trabajo

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

### 13.8 Panel de Fuera de Servicio

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

### 13.9 Registro de Siniestros

**Ruta:** `Mantenimiento > Siniestros`

Lista de todos los siniestros registrados (inundaciones, incendios, daños estructurales, fallas eléctricas u otros).

**Filtros:**
- **Estado**: Abierto, Cerrado.
- **Tipo**: Inundación, Incendio, Daño Estructural, Falla Eléctrica, Otro.
- **Búsqueda libre**: Por nombre o descripción.

### 13.10 Crear Siniestro

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

### 13.11 Detalle de Siniestro

**Ruta:** `Mantenimiento > [Siniestro]`

Página de detalle con tres secciones:

**Información:** Datos del siniestro, tipo, fecha, valor del daño y estado. Botón para cambiar el estado (Abierto/Cerrado).

**Órdenes Vinculadas:** Lista de órdenes de trabajo asociadas al siniestro. Botón para vincular nuevas órdenes existentes.

**Datos de Seguro:** Información de la póliza de seguro, aseguradora y archivo digitalizado de la póliza.

### 13.12 Reportes de Mantenimiento

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

## 14. Módulo de Comunicados y Notificaciones

Este módulo gestiona toda la comunicación oficial entre la administración y los residentes. Está compuesto por dos subsistemas principales: los **comunicados formales** (circulares, avisos, boletines) y las **notificaciones automáticas** (alertas generadas por eventos de otros módulos).

### 14.1 Comunicados

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

### 14.2 Plantillas de Notificación

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

### 14.3 Cartelera Digital

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

### 14.4 Preferencias de Comunicación

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

### 14.5 Secuencia de Avisos de Mora

Configura la progresión automática de avisos para unidades en mora.

1. Vaya a **Comunicaciones → Secuencia Mora**.
2. Cada paso (Primer Aviso, Segundo Aviso, Tercer Aviso, Prejurídico) se configura con:
   - **Días después de vencimiento**: cuándo se activa este paso.
   - **Plantilla**: qué plantilla de notificación usar.
   - **Activo**: si el paso está habilitado.
3. Para **pausar la secuencia** para una unidad específica (ej. por acuerdo de pago):
   - Use el formulario *Nueva Pausa* indicando el ID de la unidad, motivo y fechas.
4. **Ejecutar Proceso de Mora**: procesa manualmente la secuencia para todas las unidades vencidas.

### 14.6 Eventos que generan notificaciones automáticas

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

## 15. Roles y Permisos

El sistema cuenta con los siguientes roles:

| Rol | Descripción |
|-----|-------------|
| **SuperAdmin** | Acceso total a todos los módulos y conjuntos. |
| **Admin** | Administrador del conjunto. Acceso completo a configuración, finanzas y residentes. |
| **Council** | Miembro del Consejo de Administración. Puede aprobar traslados presupuestales y usos del fondo. |
| **Accountant** | Contador. Acceso a contabilidad, presupuesto y reportes. |
| **Auditor** | Acceso de solo lectura a reportes financieros. |
| **Resident** | Propietario o residente. Acceso solo a su unidad, su estado de cuenta y notificaciones. |

### 15.1 Permisos por módulo

| Módulo | Admin | Council | Accountant | Auditor | Resident |
|--------|-------|---------|------------|---------|----------|
| Dashboard | Completo | Resumen | Indicadores | Reportes | Solo su unidad |
| Unidades | CRUD | Lectura | Lectura | Lectura | Su unidad |
| Propietarios | CRUD | Lectura | Lectura | Lectura | — |
| Arrendatarios | CRUD | Lectura | Lectura | Lectura | — |
| Contabilidad | CRUD | — | CRUD | Lectura | — |
| Presupuesto | CRUD | Aprobar | CRUD | Lectura | — |
| Fondo Imprevistos | CRUD | Aprobar | CRUD | Lectura | — |
| Cuotas y Cartera | CRUD | — | CRUD | Lectura | Su estado de cuenta |
| Configuración | CRUD | — | — | — | — |
| PQR | CRUD | Responder, Alertas | Responder | Lectura | Radicar, Seguimiento |
| Proveedores | CRUD | Lectura | CRUD | Lectura | — |
| Contratos | CRUD | Aprobar, Alertas | CRUD | Lectura | — |
| Mantenimiento | CRUD | Lectura | Lectura | Lectura | — |
| Comunicaciones | CRUD | Lectura | Lectura | Lectura | Cartelera, Preferencias |

---

## 16. Preguntas Frecuentes

**¿Cómo recupero mi contraseña?**
Actualmente debe contactar al administrador del sistema para restablecerla.

**¿Por qué no puedo guardar una unidad?**
Verifique que la suma de coeficientes de todas las unidades activas sea exactamente 100.0000%. El sistema no permite guardar si hay diferencia.

**¿Cómo se calcula la cuota de administración?**
`Cuota = Presupuesto Mensual Total × (Coeficiente de la Unidad / 100)`.

**¿Qué pasa si una unidad no paga?**
El sistema calcula intereses de mora diariamente. Después de 30 días pasa a etapa prejurídica y después de 60 a jurídica.

**¿Puedo editar un asiento contable ya contabilizado?**
No. Los asientos en estado `Final` son inmutables. Debe revertirlo (generando un asiento de reversión) y crear uno nuevo.

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

## 17. Glosario

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

## 18. Módulo de Reportes y Exportaciones

Este módulo centraliza todos los reportes del sistema. Podrá generar informes financieros, de cartera, operativos, de asamblea y anuales, consultar el historial de reportes generados, configurar reportes recurrentes, construir el informe anual de gestión, personalizar la apariencia de los PDF y controlar el acceso según su rol.

### 18.1 Catálogo de Reportes (página /reports)

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
| Libro Diario | Listado cronológico de todos los asientos contables del período. | PDF, Excel |
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

### 18.2 Historial de Reportes (página /reports/history)

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

### 18.3 Reportes Recurrentes (página /reports/recurring)

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

### 18.4 Informe Anual de Gestión (página /reports/annual)

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

### 18.5 Plantillas PDF (página /reports/templates)

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

### 18.6 Roles y Permisos

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
