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
11. [Roles y Permisos](#11-roles-y-permisos)
12. [Preguntas Frecuentes](#12-preguntas-frecuentes)
13. [Glosario](#13-glosario)

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

## 11. Roles y Permisos

El sistema cuenta con los siguientes roles:

| Rol | Descripción |
|-----|-------------|
| **SuperAdmin** | Acceso total a todos los módulos y conjuntos. |
| **Admin** | Administrador del conjunto. Acceso completo a configuración, finanzas y residentes. |
| **Council** | Miembro del Consejo de Administración. Puede aprobar traslados presupuestales y usos del fondo. |
| **Accountant** | Contador. Acceso a contabilidad, presupuesto y reportes. |
| **Auditor** | Acceso de solo lectura a reportes financieros. |
| **Resident** | Propietario o residente. Acceso solo a su unidad, su estado de cuenta y notificaciones. |

### 11.1 Permisos por módulo

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

---

## 12. Preguntas Frecuentes

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

---

## 13. Glosario

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

---

> **Nota:** Este manual se actualizará a medida que se agreguen nuevos módulos y funcionalidades al sistema. Para sugerencias o reportes de errores, contacte al equipo de desarrollo.

---
*Documento generado el Junio 2026 — Softcoinp ERP*
