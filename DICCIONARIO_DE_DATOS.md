# Diccionario de Datos y Guía de Uso - Softcoinp ERP

Este documento detalla cada uno de los módulos principales del ERP, especificando la naturaleza de sus campos, reglas de negocio asociadas y la forma correcta de llenarlos en el sistema.

---

## 1. Configuración del Conjunto (Tenant Settings)
Este módulo almacena los datos de la copropiedad y del representante legal.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Tipo de Documento (Rep. Legal)** | `IdentityDocumentType` | Sí | El tipo de documento del representante (CC, NIT, CE, Pasaporte). La selección aquí determina si se calcula el DV. |
| **Número de Documento** | `string` | Sí | Número de identificación. Si el tipo es **NIT (2)**, el sistema calculará y mostrará automáticamente el Dígito de Verificación (DV). Si es **CC (1)**, no se requerirá DV. |
| **Nombre del Representante** | `string` | Sí | Nombre completo de la persona natural o razón social si aplica. |
| **Fecha de Inicio** | `date` | Sí | Fecha en que inició funciones el representante legal. |
| **Fecha de Fin** | `date` | No | Fecha en la que cesan las funciones (si es indefinido se puede omitir). |
| **Porcentaje Fondo de Imprevistos** | `decimal(5,2)` | Sí | Porcentaje mínimo sobre los ingresos mensuales destinado al fondo de imprevistos (Ley 675 Art. 35). Mínimo legal: 1%. |

---

## 2. Módulo de Unidades (Catálogo de Propiedades)
Representa el inventario físico de las propiedades (apartamentos, casas, locales) y es la columna vertebral de la facturación y la asamblea.

### Invariante Fundamental
> [!IMPORTANT]
> **El coeficiente de copropiedad de todas las unidades "Activas" o "En Proceso" del conjunto debe sumar exactamente 100.00%.**
> Si la suma es diferente de 100%, la base de datos lanzará un error SQL `45000` bloqueando la transacción para proteger la integridad matemática.

### Campos Principales

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Identificador (`identifier`)** | `string` max 50 | Sí | Nombre de la unidad tal como se conoce en el conjunto. Ej. "A-101", "Casa 4", "Local 3B". |
| **Tipo de Unidad (`unitTypeName`)** | `string` max 100 | Sí | Clasificación arquitectónica. Ej. "Apartment", "House", "Commercial Locale". |
| **Torre o Bloque (`towerOrBlock`)** | `string` max 50 | Sí | Agrupación física de la unidad. Ej. "Torre 1", "Bloque A". |
| **Nivel o Piso (`floorLevel`)** | `int` | Sí | Piso en el que se ubica. Si es casa de un solo nivel, poner `1`. |
| **Área Privada (`privateArea`)** | `decimal(18,2)` | Sí | Área construida o privada en metros cuadrados (m²). |
| **Área de Balcón (`balconyArea`)** | `decimal(18,2)` | Sí | Área del balcón o terraza en metros cuadrados (m²). Poner `0` si no aplica. |
| **Coeficiente (`coproprietyCoefficient`)** | `decimal(18,4)` | Sí | Porcentaje de participación. Este valor dicta el cobro de la cuota de administración y los votos en asamblea. La sumatoria global debe dar `100.0000`. |
| **Estado (`status`)** | `Enum` (int) | Sí | Estado físico de ocupación. Valores posibles:<br>• `1` Activa y Ocupada<br>• `2` Activa y Desocupada<br>• `3` En Proceso de Entrega<br>• `4` En Litigio<br>• `5` Inactiva |
| **Tiene Parqueadero Privado (`hasPrivateParking`)** | `boolean` | Sí | Marca si la unidad tiene un parqueadero asignado por escritura pública. |
| **Identificador Parqueadero (`parkingIdentifier`)** | `string` max 50 | Cond. | Si la anterior es `true`, debe especificarse qué parqueadero es (Ej. "P-23"). |
| **Tiene Cuarto Útil (`hasAssignedStorage`)** | `boolean` | Sí | Marca si la unidad tiene bodega/cuarto útil o depósito asignado. |
| **Identificador Cuarto Útil (`storageIdentifier`)** | `string` max 50 | Cond. | Si la anterior es `true`, especificar el número de la bodega (Ej. "B-12"). |
| **Observaciones Internas (`internalObservations`)** | `string` max 1000 | No | Notas administrativas no visibles por los residentes sobre la unidad. |

---

## 3. Módulo de Residentes y Propietarios

### 3.1 Propietarios (`Owner`)
Persona natural o jurídica que tiene derechos de propiedad registrados sobre una o varias unidades.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Tipo de Propietario (`ownerType`)** | `Enum` | Sí | `1` = Persona Natural · `2` = Persona Jurídica. Determina qué campos adicionales se muestran. |
| **Tipo de Documento (`documentType`)** | `Enum` | Sí | `1` CC · `2` CE (Cédula Extranjería) · `3` NIT · `4` Pasaporte · `5` PEP · `6` PPT. Para NIT se calcula el DV automáticamente. |
| **Número de Documento (`documentNumber`)** | `string` max 50 | Sí | CC/NIT: solo dígitos, máx 10 caracteres. Otros tipos: alfanumérico, máx 50 caracteres. Único por tenant. |
| **Dígito de Verificación (`verificationDigit`)** | `string` max 2 | Cond. | Solo aplica si `documentType = NIT`. Se calcula automáticamente con el algoritmo de módulo 11. Solo lectura en el formulario. |
| **Nombre o Razón Social (`fullNameOrCompanyName`)** | `string` max 300 | Sí | Nombre completo de la persona natural o razón social de la empresa. Cada palabra inicia en mayúscula. |
| **Correo Electrónico (`email`)** | `string` max 256 | Sí | Correo principal de contacto. Único por tenant. |
| **Teléfono Principal (`mainPhone`)** | `string` max 20 | Sí | Formato libre, solo dígitos, `+`, `-` y espacios. |
| **Teléfono Alternativo (`alternativePhone`)** | `string` max 20 | No | Segundo número de contacto. |
| **Dirección de Correspondencia (`correspondenceAddress`)** | `string` max 500 | No | Dirección física para envío de correspondencia oficial. |
| **Fecha de Nacimiento (`dateOfBirth`)** | `date` | No | Solo para personas naturales. |
| **Estado Civil (`civilStatus`)** | `string` max 30 | No | Solo para personas naturales. Ej. "Soltero", "Casado". |
| **Nombre Rep. Legal (`legalRepresentativeName`)** | `string` max 300 | Cond. | Obligatorio si `ownerType = LegalEntity`. |
| **Tipo Doc. Rep. Legal (`legalRepresentativeDocumentType`)** | `Enum` | Cond. | Obligatorio si `ownerType = LegalEntity`. Mismos valores que `documentType`. |
| **Documento Rep. Legal (`legalRepresentativeDocument`)** | `string` max 50 | Cond. | Obligatorio si `ownerType = LegalEntity`. |
| **Cargo Rep. Legal (`legalRepresentativeRole`)** | `string` max 100 | Cond. | Obligatorio si `ownerType = LegalEntity`. Ej. "Gerente General". |
| **Vencimiento Poder (`powerOfAttorneyExpiration`)** | `date` | No | Fecha de vencimiento del poder notarial si aplica. |
| **Activo (`isActive`)** | `boolean` | Sí | Indica si el propietario está activo en el sistema. |

### 3.2 Vinculación Unidad–Propietario (`UnitOwner`)
Tabla asociativa que registra la asignación de un propietario a una unidad con sus condiciones de propiedad.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Unidad (`unitId`)** | `Guid` FK | Sí | Referencia a la unidad sobre la que recae la propiedad. |
| **Propietario (`ownerId`)** | `Guid` FK | Sí | Referencia al propietario registrado. |
| **Porcentaje de Propiedad (`ownershipPercentage`)** | `decimal(5,2)` | Sí | Porcentaje que le corresponde al propietario dentro de la unidad (útil en copropiedades entre múltiples personas). La suma de todos los propietarios activos de la unidad no debe exceder 100%. |
| **Es Vocero (`isSpokesperson`)** | `boolean` | Sí | Solo puede haber un vocero activo por unidad. Si se designa uno nuevo, el anterior pierde esa condición. |
| **Reside en la Unidad (`residesInUnit`)** | `boolean` | Sí | Indica si el propietario habita físicamente la unidad o la tiene arrendada/vacía. |
| **Fecha de Inicio (`startDate`)** | `date` | Sí | Fecha desde la que es válida esta asignación de propiedad. |
| **Fecha de Fin (`endDate`)** | `date` | No | Fecha en la que cesa la propiedad (transferencia). Nulo si la propiedad está vigente. |

### 3.3 Arrendatarios (`TenantResident`)
Persona que ocupa temporalmente una unidad mediante contrato de arrendamiento.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Unidad (`unitId`)** | `Guid` FK | Sí | Unidad que ocupa el arrendatario. Solo puede haber un arrendatario activo por unidad. |
| **Tipo de Documento (`documentType`)** | `Enum` | Sí | Mismos valores que en Propietario (CC, CE, Pasaporte, PEP, PPT). No aplica NIT. |
| **Número de Documento (`documentNumber`)** | `string` max 50 | Sí | CC: solo dígitos máx 10. Otros: alfanumérico máx 50. |
| **Nombre Completo (`fullName`)** | `string` max 300 | Sí | Cada palabra inicia en mayúscula. |
| **Correo Electrónico (`email`)** | `string` max 256 | Sí | Correo de contacto del arrendatario. |
| **Teléfono (`phone`)** | `string` max 20 | Sí | Solo dígitos, `+`, `-` y espacios. |
| **Fecha de Inicio Contrato (`leaseStartDate`)** | `date` | Sí | Fecha desde la que el contrato de arrendamiento está vigente. |
| **Fecha de Terminación Contrato (`leaseEndDate`)** | `date` | No | Fecha de vencimiento del contrato. Si se omite, el contrato es indefinido. El sistema calcula automáticamente los días hasta el vencimiento. |
| **Nombre Inmobiliaria (`realEstateAgentName`)** | `string` max 200 | No | Nombre de la inmobiliaria o intermediario del arrendamiento si aplica. |
| **Teléfono Inmobiliaria (`realEstateAgentPhone`)** | `string` max 20 | No | Teléfono del intermediario. |
| **Autorizado a Pagar Admin (`authorizedToPayAdmin`)** | `boolean` | Sí | Indica si el arrendatario puede realizar pagos de cuota de administración directamente (en lugar del propietario). |
| **Activo (`isActive`)** | `boolean` | Sí | `false` cuando el contrato se termina y el arrendatario sale. |

### 3.4 Grupo de Convivencia (`CohabitationGroupMember`)
Registro de todas las personas y mascotas que habitan en una unidad.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Unidad (`unitId`)** | `Guid` FK | Sí | Unidad donde reside el miembro. |
| **Nombre (`fullNameOrPetName`)** | `string` max 200 | Sí | Nombre completo de la persona o nombre de la mascota. |
| **Parentesco / Relación (`relationship`)** | `string` max 100 | Sí | Para personas: "Cónyuge", "Hijo", "Empleado Doméstico", etc. Para mascotas: "Mascota". |
| **Fecha de Nacimiento (`dateOfBirth`)** | `date` | No | Solo aplica a personas. |
| **Es Menor de Edad (`isMinor`)** | `boolean` | No | El sistema puede inferirlo de `dateOfBirth`, pero se puede declarar explícitamente. |
| **Es Mascota (`isPet`)** | `boolean` | Sí | Determina si aplican los campos específicos de mascota. |
| **Especie (`petSpecies`)** | `string` max 100 | Cond. | Obligatorio si `isPet = true`. Ej. "Perro", "Gato". |
| **Raza (`petBreed`)** | `string` max 100 | No | Raza de la mascota. |
| **Registro Sanitario (`petSanitaryRegistration`)** | `string` max 100 | No | Número del carnet de vacunación u otro registro sanitario. |
| **Activo (`isActive`)** | `boolean` | Sí | `false` cuando el miembro ya no reside en la unidad. |

---

## 4. Módulo de Plan Contable y Presupuesto

### 4.1 Cuentas Contables (`AccountingAccount`)
Catálogo jerárquico de cuentas basado en la **Resolución 029 de 2019** del Consejo Técnico de la Contaduría Pública, adaptado para propiedades horizontales sin ánimo de lucro.

> [!IMPORTANT]
> **Las cuentas del estándar oficial (`isOfficialStandard = true`) no pueden ser modificadas ni eliminadas por ningún usuario.** Solo se pueden agregar cuentas auxiliares de nivel 4 (bajo cuentas de 4 dígitos) y nivel 5 (bajo cuentas de 6 dígitos). El código de la cuenta auxiliar debe comenzar con el código de su cuenta padre.

#### Jerarquía de códigos

| Nivel | Longitud código | Ejemplo | Descripción |
|-------|----------------|---------|-------------|
| 1 | 1 dígito | `1` | Clase (Activo, Pasivo, etc.) |
| 2 | 2 dígitos | `11` | Grupo |
| 3 | 4 dígitos | `1105` | Cuenta |
| 4 | 6 dígitos | `110501` | Subcuenta (auxiliar) |
| 5 | 8 dígitos | `11050101` | Auxiliar de segundo orden |

#### Grupos principales del plan de cuentas (Resolución 029)

| Código | Nombre | Categoría | Naturaleza |
|--------|--------|-----------|-----------|
| `1` | Activo | Asset | Débito |
| `1105` | Caja | Asset | Débito |
| `1110` | Bancos | Asset | Débito |
| `1305` | Cuotas de Administración (Cartera) | Asset | Débito |
| `2` | Pasivo | Liability | Crédito |
| `2335` | Costos y Gastos por Pagar | Liability | Crédito |
| `2505` | Salarios por Pagar | Liability | Crédito |
| `3` | Patrimonio (Fondo Social) | Equity | Crédito |
| `3105` | Fondo Social Efectivo | Equity | Crédito |
| `3205` | Fondo de Imprevistos Ley 675 | Equity | Crédito |
| `4` | Ingresos | Income | Crédito |
| `4105` | Cuotas de Administración Ordinarias | Income | Crédito |
| `4110` | Cuotas de Administración Extraordinarias | Income | Crédito |
| `4230` | Multas y Sanciones | Income | Crédito |
| `5` | Gastos | Expense | Débito |
| `5110` | Honorarios | Expense | Débito |
| `5135` | Servicios Públicos | Expense | Débito |
| `5140` | Vigilancia y Seguridad | Expense | Débito |
| `5145` | Mantenimiento y Conservación | Expense | Débito |
| `5196` | Aporte Fondo de Imprevistos | Expense | Débito |

#### Campos de la entidad

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Código (`code`)** | `string` max 20 | Sí | Código numérico jerárquico. Único dentro del tenant. El código de una subcuenta debe iniciar con el código de su cuenta padre. |
| **Nombre (`name`)** | `string` max 100 | Sí | Nombre descriptivo de la cuenta. Editable solo en cuentas auxiliares (no oficiales). |
| **Categoría (`category`)** | `Enum` | Sí | `Asset` · `Liability` · `Equity` · `Income` · `Expense`. Las cuentas auxiliares heredan la categoría de su cuenta padre. |
| **Naturaleza (`nature`)** | `Enum` | Sí | `Debit` (saldo normal débito) · `Credit` (saldo normal crédito). Las cuentas auxiliares heredan la naturaleza de su cuenta padre. |
| **Es Cuenta de Agrupación (`isGroup`)** | `boolean` | Sí | `true` = la cuenta agrupa subcuentas y no recibe movimientos directos. `false` = cuenta de movimiento, acepta asientos contables. |
| **Activa (`isActive`)** | `boolean` | Sí | Las cuentas inactivas no aparecen en selectores de presupuesto ni de asientos. |
| **Es Estándar Oficial (`isOfficialStandard`)** | `boolean` | Sí | `true` = viene precargada con la Resolución 029 y no puede modificarse. `false` = cuenta auxiliar creada por el conjunto. |

### 4.2 Asientos Contables (`AccountingEntry`)
Registro de cada movimiento en el libro diario. Es la fuente de verdad para calcular la ejecución presupuestal en tiempo real.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Cuenta Contable (`accountingAccountId`)** | `Guid` FK | Sí | Debe ser una cuenta de movimiento (`isGroup = false`). |
| **Débito (`debit`)** | `decimal(18,2)` | Sí | Valor debitado en la cuenta. Poner `0` si el movimiento es solo crédito. |
| **Crédito (`credit`)** | `decimal(18,2)` | Sí | Valor acreditado en la cuenta. Poner `0` si el movimiento es solo débito. |
| **Fecha del Asiento (`entryDate`)** | `datetime` | Sí | Fecha en que ocurrió el movimiento económico (no la fecha de registro). Determina en qué período fiscal se contabiliza. |
| **Descripción (`description`)** | `string` max 500 | No | Texto libre que explica el concepto del movimiento. |
| **Referencia (`reference`)** | `string` max 100 | No | Número del comprobante de egreso, ingreso o recibo que origina el asiento. Ej. "CE-00123", "LIQ-2025-06". |

### 4.3 Presupuesto Anual (`Budget`)
Instrumento de planeación financiera aprobado por la Asamblea Ordinaria de Copropietarios. Solo puede existir **un presupuesto activo por período fiscal**.

> [!IMPORTANT]
> Un presupuesto en estado **Borrador** puede editarse libremente. Una vez **Activado** (requiere acta de asamblea), no puede editarse directamente: cualquier cambio debe hacerse mediante un Traslado o Adición presupuestal. Un presupuesto **Cerrado** es inmutable.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Período Fiscal (`fiscalPeriod`)** | `int` | Sí | Año calendario al que corresponde el presupuesto. Ej. `2025`. Solo puede existir un presupuesto activo por año. |
| **Número de Acta (`meetingActNumber`)** | `string` max 100 | Sí* | Número del acta de asamblea que aprobó el presupuesto. *Obligatorio al activar (puede quedar vacío en borrador). |
| **Fecha de Aprobación (`approvalDate`)** | `datetime` | Sí* | Fecha en que la asamblea aprobó el presupuesto. *Obligatoria al activar. |
| **Estado (`status`)** | `Enum` | Sí | `Draft` = Borrador · `Active` = Activo (aprobado) · `Closed` = Cerrado (período terminado). |
| **Creado Por (`createdByUserId`)** | `string` FK | Sí | ID del usuario que creó el borrador. |

#### Modos de creación del presupuesto
- **Manual**: se ingresan los valores cuenta por cuenta desde cero.
- **Copia del período anterior**: el sistema toma el presupuesto aprobado del año anterior y aplica un ajuste porcentual global o diferenciado por cuenta.

### 4.4 Detalle del Presupuesto (`BudgetDetail`)
Asignación de valor aprobado a cada cuenta de ingreso o gasto dentro de un presupuesto.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Presupuesto (`budgetId`)** | `Guid` FK | Sí | Presupuesto al que pertenece este rubro. |
| **Cuenta Contable (`accountingAccountId`)** | `Guid` FK | Sí | Debe ser una cuenta de movimiento (`isGroup = false`) de categoría `Income` o `Expense`. Una misma cuenta solo puede aparecer una vez por presupuesto (índice único). |
| **Valor Aprobado (`approvedValue`)** | `decimal(18,2)` | Sí | Monto que la asamblea aprobó para esta cuenta en el período fiscal. Solo afectable después de la activación mediante movimientos presupuestales. |
| **Observaciones (`observations`)** | `string` max 500 | No | Notas sobre el criterio utilizado para definir el valor de este rubro. |

### 4.5 Movimientos Presupuestales (`BudgetMovement`)
Registro de **traslados** (mover entre rubros) y **adiciones** (aumentar el techo aprobado) sobre un presupuesto activo. Cada movimiento requiere respaldo formal en acta.

> [!NOTE]
> **Traslado**: mueve saldo entre dos cuentas del **mismo grupo** (Gasto → Gasto). Puede ser aprobado por el Consejo de Administración.<br>
> **Adición**: incrementa el total del presupuesto de gastos. Requiere aprobación de **Asamblea Extraordinaria** porque implica aumentar cuotas o usar reservas.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Presupuesto (`budgetId`)** | `Guid` FK | Sí | Debe ser un presupuesto en estado `Active`. |
| **Tipo de Movimiento (`movementType`)** | `Enum` | Sí | `Transfer` = Traslado entre cuentas · `Addition` = Adición presupuestal. |
| **Cuenta Origen (`sourceAccountId`)** | `Guid` FK | Cond. | Solo obligatorio para `Transfer`. La cuenta origen y destino deben pertenecer a la misma categoría. El sistema valida que el saldo disponible en la cuenta origen sea suficiente. |
| **Cuenta Destino (`destinationAccountId`)** | `Guid` FK | Sí | Cuenta que recibe el monto. Debe ser cuenta de movimiento activa. |
| **Monto (`amount`)** | `decimal(18,2)` | Sí | Valor del movimiento. Debe ser mayor a cero. |
| **Justificación (`justification`)** | `string` max 1000 | Sí | Explicación técnica o económica del motivo del cambio presupuestal. |
| **Tipo de Aprobación (`approvalType`)** | `Enum` | Sí | `Council` = Consejo de Administración (solo para traslados) · `Assembly` = Asamblea (obligatorio para adiciones). |
| **Número de Acta (`meetingActNumber`)** | `string` max 100 | Sí | Número del acta del consejo o asamblea que aprobó el movimiento. |
| **Fecha de Aprobación (`approvalDate`)** | `datetime` | Sí | Fecha en que fue aprobado el movimiento. |

### 4.6 Fondo de Imprevistos (`ContingencyFund`)
Reserva obligatoria según el **Artículo 35 de la Ley 675 de 2001**. Se constituye con un porcentaje mínimo del 1% de los ingresos del período y solo puede usarse para expensas imprevistas o de urgencia con aprobación del consejo.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Saldo Actual (`currentBalance`)** | `decimal(18,2)` | Automático | Calculado por el sistema. Se incrementa con cada aporte mensual y se reduce con cada uso aprobado. No editable manualmente. |

### 4.7 Aportes al Fondo de Imprevistos (`ContingencyFundContribution`)
Registro de cada aporte mensual liquidado al fondo. El sistema genera automáticamente el asiento contable correspondiente (Débito 5196 / Crédito 3205).

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Período (`period`)** | `string` max 7 | Sí | Formato `YYYY-MM`. Ej. `2025-06`. No puede liquidarse dos veces el mismo período (índice único por tenant + período). |
| **Monto Aportado (`amount`)** | `decimal(18,2)` | Automático | Calculado como `incomeBase × (percentage / 100)`. |
| **Base de Ingresos (`incomeBase`)** | `decimal(18,2)` | Automático | Suma de todos los asientos crédito menos débito en cuentas de categoría `Income` durante el período. |
| **Porcentaje Aplicado (`percentage`)** | `decimal(5,2)` | Automático | Porcentaje vigente configurado en la cuenta del conjunto al momento de la liquidación. |
| **Fecha de Liquidación (`contributionDate`)** | `datetime` | Automático | Fecha y hora en que se ejecutó la liquidación mensual. |
| **Referencia Asiento (`accountingRecordId`)** | `Guid` FK | Automático | Referencia al asiento contable de gasto (Débito 5196) generado por la liquidación. |

### 4.8 Usos del Fondo de Imprevistos (`ContingencyFundUsage`)
Registro de cada retiro del fondo. El sistema genera automáticamente el asiento contable (Débito 3205 / Crédito 1110).

> [!WARNING]
> El uso del fondo requiere aprobación previa del Consejo de Administración registrada en el sistema **antes** de permitir el egreso. El sistema valida que el saldo sea suficiente antes de procesar la operación.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Monto (`amount`)** | `decimal(18,2)` | Sí | Valor a retirar. Debe ser mayor a cero y no puede superar el saldo actual del fondo. |
| **Justificación (`justification`)** | `string` max 1000 | Sí | Descripción técnica de la urgencia o imprevisto que origina el retiro. |
| **Acta de Aprobación del Consejo (`councilApprovalActNumber`)** | `string` max 100 | Sí | Número del acta del Consejo de Administración que autorizó el retiro. |
| **Fecha de Aprobación (`approvalDate`)** | `datetime` | Sí | Fecha en que el consejo aprobó el retiro. |
| **Referencia Asiento (`accountingRecordId`)** | `Guid` FK | Automático | Referencia al asiento contable de patrimonio (Débito 3205) generado por el retiro. |

---

## 5. Módulo de Cuotas y Cartera

> [!IMPORTANT]
> Este módulo gestiona el ciclo completo de ingresos de la copropiedad: liquidación mensual de cuotas ordinarias, administración y cartera, cobros individuales, imputación de pagos, intereses de mora, acuerdos de pago y certificados de paz y salvo.
>
> **Regla de inmutabilidad**: Los registros financieros (cuotas, pagos, intereses capitalizados) **no tienen soft delete**. Una vez creados, solo pueden modificarse mediante asientos de ajuste o compensación para garantizar la integridad contable.

### 5.1 Período de Liquidación (`BillingPeriod`)
Representa un mes calendario de facturación ordinaria. Se crea en estado `Draft` y se procesa para generar las cuotas individuales por unidad.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Período (`period`)** | `string` max 7 | Sí | Formato `YYYY-MM`. Ej. `2026-06`. Único por tenant (no puede existir dos liquidaciones para el mismo período). |
| **Total Presupuesto Mensual (`monthlyBudgetTotal`)** | `decimal(18,2)` | Automático | Valor del presupuesto anual dividido en 12 meses tomado al momento de la liquidación. |
| **Fecha de Corte (`cutoffDate`)** | `datetime` | Sí | Fecha límite para incluir cargos del período. |
| **Fecha de Vencimiento (`paymentDueDate`)** | `datetime` | Sí | Fecha tope para pago sin intereses de mora. |
| **Estado (`status`)** | `Enum` | Sí | `Draft` = Borrador · `Executed` = Ejecutado (cuotas generadas) · `Closed` = Cerrado contablemente. |
| **Ajuste de Redondeo (`roundingAdjustment`)** | `decimal(18,2)` | Automático | Diferencia generada por el redondeo individual de cuotas. Se registra como ajuste en el período. |
| **Notas (`notes`)** | `string` max 1000 | No | Observaciones sobre la liquidación. |

### 5.2 Cuota Ordinaria (`UnitFee`)
Cuota de administración individual generada para cada unidad en cada período de liquidación. Es la principal fuente de ingresos de la copropiedad.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Período de Liquidación (`billingPeriodId`)** | `Guid` FK | Sí | Referencia al período que originó la cuota. |
| **Unidad (`unitId`)** | `Guid` FK | Sí | Unidad propietaria de la cuota. |
| **Valor de la Cuota (`feeValue`)** | `decimal(18,2)` | Sí | Calculado como `presupuestoMensual × (coeficienteUnidad / 100)`. Redondeado a 2 decimales. |
| **Fecha de Vencimiento (`dueDate`)** | `datetime` | Sí | Fecha tope para pago oportuno. Heredada del período. |
| **Estado (`status`)** | `Enum` | Sí | `Pending` · `PartiallyPaid` · `FullyPaid` · `Overdue` (automático cuando se pasa la fecha de vencimiento sin pago completo). |
| **Monto Pagado (`paidAmount`)** | `decimal(18,2)` | Automático | Suma de imputaciones de pago aplicadas a esta cuota. |
| **Saldo Pendiente (`balanceAmount`)** | `decimal(18,2)` | Automático | `feeValue - paidAmount`. Se actualiza con cada imputación de pago. |

### 5.3 Cuota Extraordinaria (`ExtraordinaryFee`)
Cuota adicional aprobada en Asamblea General de Copropietarios para cubrir gastos no presupuestados (Ley 675 Art. 46).

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Nombre (`name`)** | `string` max 200 | Sí | Nombre descriptivo de la cuota. Ej. "Impermeabilización Fachada 2026". |
| **Descripción (`description`)** | `string` max 1000 | No | Detalle del propósito de la cuota. |
| **Número de Acta (`meetingActNumber`)** | `string` max 100 | Sí | Número del acta de la asamblea que aprobó la cuota. |
| **Monto Total (`totalAmount`)** | `decimal(18,2)` | Sí | Valor total a recaudar aprobado por la asamblea. |
| **Número de Cuotas (`numberOfInstallments`)** | `int` | Sí | Cantidad de cuotas en que se fracciona el pago (mínimo 1). |
| **Período de Inicio (`startPeriod`)** | `string` max 7 | Sí | Mes desde el cual se comienza a cobrar. Formato `YYYY-MM`. |
| **Tipo de Distribución (`distributionType`)** | `Enum` | Sí | `AllByCoefficient` = Se distribuye por coeficiente de copropiedad · `SpecificGroup` = Solo a un grupo específico de unidades. |
| **Estado (`status`)** | `Enum` | Sí | `Pending` · `Active` · `Completed` · `Cancelled`. |

### 5.4 Distribución de Cuota Extraordinaria (`ExtraordinaryFeeDistribution`)
Registro individual por unidad y por cuota (si aplican múltiples contados) de una cuota extraordinaria.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Cuota Extraordinaria (`extraordinaryFeeId`)** | `Guid` FK | Sí | Referencia a la cuota extraordinaria. |
| **Unidad (`unitId`)** | `Guid` FK | Sí | Unidad sobre la que recae el cobro. |
| **Número de Cuota (`installmentNumber`)** | `int` | Sí | Número correlativo del contado (1, 2, 3…). |
| **Monto (`amount`)** | `decimal(18,2)` | Sí | Valor de este contado para esta unidad. |
| **Fecha de Vencimiento (`dueDate`)** | `datetime` | Sí | Fecha tope para el pago de este contado. |
| **Estado (`status`)** | `Enum` | Sí | `Pending` · `PartiallyPaid` · `FullyPaid` · `Overdue`. |
| **Monto Pagado (`paidAmount`)** | `decimal(18,2)` | Automático | Actualizado vía imputación de pagos. |
| **Saldo Pendiente (`balanceAmount`)** | `decimal(18,2)` | Automático | `amount - paidAmount`. |

### 5.5 Cobro Individual (`IndividualCharge`)
Multas, daños a bienes comunes, servicios adicionales u otros cobros particulares a una unidad (Art. 58 Ley 675).

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Unidad (`unitId`)** | `Guid` FK | Sí | Unidad responsable del cobro. |
| **Tipo de Cobro (`chargeType`)** | `Enum` | Sí | `Fine` = Multa · `Damage` = Daño a bien común · `ParkingFee` = Parqueadero visitante · `Other` = Otros. |
| **Concepto (`concept`)** | `string` max 200 | Sí | Título breve del cobro. Ej. "Multa por ruido excesivo". |
| **Monto (`amount`)** | `decimal(18,2)` | Sí | Valor del cobro. Debe ser mayor a cero. |
| **Fecha del Cobro (`chargeDate`)** | `datetime` | Sí | Fecha en que se generó el cobro. |
| **Descripción (`description`)** | `string` max 1000 | No | Detalle de los hechos que motivan el cobro. |
| **Número de Acta de Referencia (`referenceActNumber`)** | `string` max 100 | No | Acta del consejo o comité que impuso el cobro. |
| **En Disputa (`isDisputed`)** | `boolean` | No | `true` si el propietario ha impugnado formalmente el cobro. |
| **Motivo de Disputa (`disputeReason`)** | `string` max 1000 | Cond. | Obligatorio si `isDisputed = true`. |
| **Estado (`status`)** | `Enum` | Sí | `Pending` · `Paid` · `Waived` (condonado) · `Disputed`. |
| **Monto Pagado (`paidAmount`)** | `decimal(18,2)` | Automático | Actualizado vía imputación de pagos. |
| **Saldo Pendiente (`balanceAmount`)** | `decimal(18,2)` | Automático | `amount - paidAmount`. |

### 5.6 Pago (`Payment`)
Registro de un pago recibido de una unidad. Puede cubrir múltiples conceptos (cuotas ordinarias, extraordinarias, intereses, cobros individuales).

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Unidad (`unitId`)** | `Guid` FK | Sí | Unidad que realiza el pago. |
| **Fecha de Pago (`paymentDate`)** | `datetime` | Sí | Fecha en que se recibe el pago. |
| **Monto (`amount`)** | `decimal(18,2)` | Sí | Valor total recibido. Debe coincidir con la suma de las imputaciones. |
| **Medio de Pago (`paymentMethod`)** | `Enum` | Sí | `Cash` · `Transfer` · `Check` · `Online`. |
| **Número de Referencia (`referenceNumber`)** | `string` max 100 | No | Número de consignación, cheque o transacción. |
| **Notas (`notes`)** | `string` max 500 | No | Observaciones sobre el pago. |
| **Recibido Por (`receivedByUserId`)** | `string` FK | Automático | ID del usuario que registró el pago. |
| **Anticipo (`advanceAmount`)** | `decimal(18,2)` | Automático | Excedente después de imputar todas las obligaciones vencidas. Se aplica a períodos futuros. |

### 5.7 Imputación de Pago (`PaymentAllocation`)
Línea individual que detalla cómo se aplicó un pago a una obligación específica (cuota, interés, cobro).

> [!IMPORTANT]
> El orden de imputación es **fijo e inmodificable** por principios contables y jurídicos colombianos:
> 1. **Intereses de mora capitalizados** más antiguos
> 2. **Capital vencido** en orden cronológico (primero lo más antiguo)
> 3. **Período corriente** (cuota del mes vigente)
> 4. El **excedente** se registra como anticipo del período siguiente

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Pago (`paymentId`)** | `Guid` FK | Sí | Pago al que pertenece esta imputación. |
| **Tipo de Origen (`sourceType`)** | `string` | Sí | Identifica la tabla origen: `UnitFee`, `ExtraordinaryFeeDistribution`, `IndividualCharge`, `LateInterest`. |
| **ID de Origen (`sourceId`)** | `Guid` | Cond. | ID del registro en la tabla origen. Opcional para ajustes generales. |
| **Monto Imputado (`amount`)** | `decimal(18,2)` | Sí | Valor aplicado a esta obligación específica. |
| **Tipo de Asignación (`allocationType`)** | `Enum` | Sí | `Interest` · `Capital` · `Advance`. |

### 5.8 Interés de Mora (`LateInterest`)
Registro de intereses de mora calculados sobre obligaciones vencidas. La capitalización ocurre formalmente al momento del pago o cierre del período.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Unidad (`unitId`)** | `Guid` FK | Sí | Unidad deudora. |
| **Tipo de Origen (`sourceType`)** | `string` | Sí | `UnitFee`, `ExtraordinaryFeeDistribution` o `IndividualCharge`. |
| **ID de Origen (`sourceId`)** | `Guid` | Sí | ID del registro que generó el interés. |
| **Período (`period`)** | `string` max 7 | Sí | Período en que se calculó el interés. Formato `YYYY-MM`. |
| **Monto Base (`baseAmount`)** | `decimal(18,2)` | Sí | Saldo de capital sobre el que se calculó el interés. |
| **Tasa Diaria (`dailyRate`)** | `decimal(18,6)` | Sí | `tasaMensual / 30 / 100`. La tasa mensual se toma de `TenantConfiguration.LatePaymentInterestRate`. |
| **Días de Mora (`daysOverdue`)** | `int` | Sí | Número de días entre la fecha de vencimiento y la fecha de cálculo. |
| **Monto Calculado (`calculatedAmount`)** | `decimal(18,2)` | Sí | `baseAmount × dailyRate × daysOverdue`. Redondeado a 2 decimales. |
| **Está Capitalizado (`isCapitalized`)** | `boolean` | Sí | `true` cuando el interés ha sido formalmente incorporado al capital para efectos de cobro judicial. |

### 5.9 Acuerdo de Pago (`PaymentAgreement`)
Instrumento formal aprobado por el Consejo de Administración para facilitar el pago de obligaciones vencidas, incluyendo la posibilidad de condonar parcialmente los intereses de mora.

> [!IMPORTANT]
> **Reglas de negocio:**
> - Solo puede existir **un acuerdo activo por unidad** a la vez.
> - La condonación de intereses no puede exceder el porcentaje máximo configurado en el tenant.
> - El incumplimiento se detecta automáticamente cuando una cuota supera los **5 días de mora**.
> - Al crearse, las obligaciones incluidas se marcan como cubiertas por el acuerdo.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Unidad (`unitId`)** | `Guid` FK | Sí | Unidad que suscribe el acuerdo. |
| **Deuda Total Incluida (`totalDebtIncluded`)** | `decimal(18,2)` | Sí | Suma del capital e intereses incluidos en el acuerdo. |
| **Valor de la Cuota (`installmentAmount`)** | `decimal(18,2)` | Automático | `netDebt / numberOfInstallments`. |
| **Número de Cuotas (`numberOfInstallments`)** | `int` | Sí | Número de contados acordados (mínimo 1). |
| **% Condonación de Intereses (`interestForgivenessPercentage`)** | `decimal(5,2)` | Sí | Porcentaje de intereses que se condonan. |
| **Número de Acta del Consejo (`councilActNumber`)** | `string` max 100 | Sí | Acta del Consejo que aprobó el acuerdo. |
| **Estado (`status`)** | `Enum` | Sí | `Pending` · `Active` · `Completed` · `Defaulted` · `Cancelled`. |
| **Fecha de Inicio (`startedAt`)** | `datetime` | Automático | Fecha de creación del acuerdo. |
| **Fecha de Incumplimiento (`defaultedAt`)** | `datetime` | Automático | Fecha en que el sistema detectó el incumplimiento. |
| **Aceptación Digital (`digitalAcceptance`)** | `string` | Sí | Texto, código o hash que evidencia la aceptación del deudor. |

### 5.10 Cuota de Acuerdo de Pago (`AgreementInstallment`)
Cada una de las cuotas individuales que componen un acuerdo de pago.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Acuerdo de Pago (`paymentAgreementId`)** | `Guid` FK | Sí | Acuerdo al que pertenece la cuota. |
| **Número de Cuota (`installmentNumber`)** | `int` | Sí | Número correlativo dentro del acuerdo. |
| **Fecha de Vencimiento (`dueDate`)** | `datetime` | Sí | Fecha tope para el pago de esta cuota. |
| **Monto (`amount`)** | `decimal(18,2)` | Sí | Valor de la cuota. |
| **Monto Pagado (`paidAmount`)** | `decimal(18,2)` | Automático | Monto imputado a esta cuota. |
| **Estado (`status`)** | `Enum` | Sí | `Pending` · `Paid` · `Overdue` (automático a los 5 días de vencida). |
| **Fecha de Pago (`paidAt`)** | `datetime` | Cond. | Fecha en que se efectuó el pago. Nulo si está pendiente. |

### 5.11 Certificado de Paz y Salvo (`ClearanceCertificate`)
Documento oficial que certifica que una unidad no tiene obligaciones pendientes con la copropiedad a una fecha determinada.

> [!WARNING]
> **Solo se puede expedir si la unidad no tiene deuda pendiente.** Una vez expedido, no puede editarse. Se puede revocar solo si está en estado `Active`.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Unidad (`unitId`)** | `Guid` FK | Sí | Unidad a la que se expide el certificado. |
| **Número de Certificado (`certificateNumber`)** | `string` max 20 | Automático | Formato `PSS-000001`. Secuencia autoincremental por tenant. |
| **Fecha de Expedición (`issueDate`)** | `datetime` | Automático | Fecha y hora de emisión. |
| **Fecha de Vencimiento (`expirationDate`)** | `datetime` | Automático | `issueDate + validityDays`. Por defecto 30 días. |
| **Saldo a la Fecha (`balanceAtDate`)** | `decimal(18,2)` | Automático | Saldo de la unidad al momento de la expedición (debe ser 0). |
| **Estado (`status`)** | `Enum` | Sí | `Active` · `Revoked`. Se revoca si la unidad vuelve a quedar en mora. |
| **Expedido Por (`issuedByUserId`)** | `string` FK | Automático | ID del usuario que expidió el certificado. |
| **Nombre del Administrador (`signedByAdministratorName`)** | `string` max 300 | Automático | Nombre del representante legal registrado en la configuración del conjunto al momento de la expedición. |

---

## Resumen de Tablas en Base de Datos

| Tabla | Módulo | Descripción |
|-------|--------|-------------|
| `erp_tenant_configuration` | Configuración | Datos del conjunto y representante legal |
| `erp_unit_types` | Unidades | Tipos de unidad (Apartamento, Casa, Local…) |
| `erp_units` | Unidades | Catálogo de unidades del conjunto |
| `erp_unit_state_history` | Unidades | Historial de cambios de estado de unidades |
| `erp_unit_complements` | Unidades | Parqueaderos y bodegas vinculados a unidades |
| `erp_owners` | Residentes | Propietarios (persona natural o jurídica) |
| `erp_unit_owners` | Residentes | Vinculación unidad–propietario con porcentaje |
| `erp_owner_histories` | Residentes | Historial de transferencias de propiedad |
| `erp_tenant_residents` | Residentes | Arrendatarios activos e históricos |
| `erp_cohabitation_group_members` | Residentes | Personas y mascotas en cada unidad |
| `erp_contact_histories` | Residentes | Cambios de datos de contacto de propietarios |
| `erp_spokesperson_histories` | Residentes | Historial de designación de voceros por unidad |
| `erp_accounting_accounts` | Contabilidad | Plan de cuentas (Resolución 029 + auxiliares) |
| `erp_accounting_entries` | Contabilidad | Libro diario — asientos contables |
| `erp_budgets` | Presupuesto | Presupuestos anuales aprobados por asamblea |
| `erp_budget_details` | Presupuesto | Rubros del presupuesto por cuenta contable |
| `erp_budget_movements` | Presupuesto | Traslados y adiciones presupuestales |
| `erp_contingency_funds` | Presupuesto | Saldo actual del fondo de imprevistos |
| `erp_contingency_fund_contributions` | Presupuesto | Aportes mensuales al fondo |
| `erp_contingency_fund_usages` | Presupuesto | Retiros aprobados del fondo |
| `erp_billing_periods` | Cuotas y Cartera | Períodos de liquidación mensual |
| `erp_unit_fees` | Cuotas y Cartera | Cuotas ordinarias por unidad |
| `erp_extraordinary_fees` | Cuotas y Cartera | Cuotas extraordinarias aprobadas en asamblea |
| `erp_extraordinary_fee_distributions` | Cuotas y Cartera | Distribución de cuotas extra por unidad/cuota |
| `erp_individual_charges` | Cuotas y Cartera | Multas, daños y cobros individuales |
| `erp_payments` | Cuotas y Cartera | Pagos recibidos |
| `erp_payment_allocations` | Cuotas y Cartera | Imputación detallada de pagos |
| `erp_late_interests` | Cuotas y Cartera | Intereses de mora calculados/capitalizados |
| `erp_payment_agreements` | Cuotas y Cartera | Acuerdos de pago |
| `erp_agreement_installments` | Cuotas y Cartera | Cuotas individuales de acuerdos de pago |
| `erp_clearance_certificates` | Cuotas y Cartera | Paz y salvos expedidos |
