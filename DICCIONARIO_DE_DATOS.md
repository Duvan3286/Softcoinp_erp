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

### 1.1 Auditoría de Configuración (`ConfigurationAuditLog`)
Registro de cambios realizados sobre los parámetros de configuración del conjunto. Cada modificación a parámetros críticos (porcentaje de interés, porcentaje fondo, etc.) queda registrada con el valor anterior y el nuevo.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Tenant (`tenantId`)** | `string` max 255 | Automático | Identificador del conjunto al que pertenece el cambio. Se hereda del contexto de la solicitud. |
| **Marca de Tiempo (`timestamp`)** | `datetime` | Automático | Fecha y hora del cambio. |
| **Cambiado Por (`changedByUserId`)** | `string` max 450 | Automático | ID del usuario que realizó el cambio. |
| **Parámetro (`parameterName`)** | `string` max 100 | Sí | Nombre del parámetro modificado. Ej. `"LatePaymentInterestRate"`, `"ContingencyFundPercentage"`. |
| **Valor Anterior (`oldValue`)** | `string` | Automático | Valor serializado antes del cambio. |
| **Valor Nuevo (`newValue`)** | `string` | Automático | Valor serializado después del cambio. |
| **Motivo (`reason`)** | `string` max 1000 | No | Razón opcional del cambio ingresada por el usuario. |

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
| **Porcentaje de Propiedad (`ownershipPercentage`)** | `decimal(7,4)` | Sí | Porcentaje que le corresponde al propietario dentro de la unidad (útil en copropiedades entre múltiples personas). La suma de todos los propietarios activos de la unidad no debe exceder 100%. Precisión aumentada a (7,4) para soportar coeficientes pequeños en conjuntos con muchas unidades. |
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

## 4. Módulo Contable

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
Registro de cada movimiento en el libro diario. Es la fuente de verdad para calcular la ejecución presupuestal y los saldos contables en tiempo real.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Período Contable (`accountingPeriodId`)** | `Guid` FK | No | Período fiscal al que pertenece el asiento. Nulo si la gestión de períodos no está activa. |
| **Número de Asiento (`entryNumber`)** | `int` | Automático | Número correlativo único por tenant. Se asigna automáticamente al crear el asiento. |
| **Tipo de Asiento (`entryType`)** | `Enum` | Sí | `Manual` = Ingresado por un usuario · `Automatic` = Generado por integración (facturación, pagos). |
| **Estado (`status`)** | `Enum` | Sí | `Draft` = Borrador (no contabilizado) · `Final` = Contabilizado (inmutable) · `Reversed` = Revertido. |
| **Fecha del Asiento (`entryDate`)** | `datetime` | Sí | Fecha en que ocurrió el movimiento económico (no la fecha de registro). Determina en qué período fiscal se contabiliza. |
| **Descripción (`description`)** | `string` max 500 | No | Texto libre que explica el concepto del movimiento. |
| **Referencia Externa (`externalReference`)** | `string` max 100 | No | Identificador del documento externo que origina el asiento. Ej. "LIQ-2026-06", "PAG-00123". |
| **Total Débito (`totalDebit`)** | `decimal(18,2)` | Automático | Suma de todos los débitos de las líneas del asiento. Debe ser igual a `totalCredit`. |
| **Total Crédito (`totalCredit`)** | `decimal(18,2)` | Automático | Suma de todos los créditos de las líneas del asiento. Debe ser igual a `totalDebit`. |
| **Creado Por (`createdByUserId`)** | `string` FK | Automático | ID del usuario que creó el asiento. |
| **Actualizado Por (`updatedByUserId`)** | `string` FK | No | ID del último usuario que modificó el asiento (solo aplica en borrador). |

> [!IMPORTANT]
> **Partida doble**: Todo asiento debe cumplir `totalDebit = totalCredit`. Esta validación se ejecuta tanto en la API como en la base de datos. Un asiento en estado `Final` es inmutable: no se pueden editar sus líneas ni cambiar su fecha. Para corregir un asiento final debe revertirse (generando un asiento de reversión).

### 4.3 Líneas de Asiento (`EntryLine`)
Cada línea individual que compone un asiento contable, registrando el movimiento en una cuenta específica.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Asiento Contable (`accountingEntryId`)** | `Guid` FK | Sí | Asiento al que pertenece la línea. |
| **Cuenta Contable (`accountingAccountId`)** | `Guid` FK | Sí | Debe ser una cuenta de movimiento (`isGroup = false`). |
| **Tercero (`thirdPartyId`)** | `string` max 50 | No | Identificador del tercero asociado al movimiento (proveedor, propietario, etc.). |
| **Débito (`debit`)** | `decimal(18,2)` | Sí | Valor debitado en la cuenta. Poner `0` si el movimiento es solo crédito. |
| **Crédito (`credit`)** | `decimal(18,2)` | Sí | Valor acreditado en la cuenta. Poner `0` si el movimiento es solo débito. |

### 4.4 Reversiones de Asientos (`EntryReversal`)
Registro de cada reversión de un asiento contable finalizado. La reversión genera un nuevo asiento con signos opuestos (débitos y créditos intercambiados).

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Asiento Original (`originalEntryId`)** | `Guid` FK | Sí | Asiento en estado `Final` que se está revirtiendo. |
| **Asiento de Reversión (`reversalEntryId`)** | `Guid` FK | Sí | Nuevo asiento generado con los valores opuestos. |
| **Motivo (`reason`)** | `string` max 500 | Sí | Explicación del motivo de la reversión. |
| **Fecha de Reversión (`reversedAt`)** | `datetime` | Automático | Fecha y hora en que se ejecutó la reversión. |
| **Reversado Por (`reversedByUserId`)** | `string` FK | Automático | ID del usuario que ejecutó la reversión. |

### 4.5 Períodos Contables (`AccountingPeriod`)
Representa un mes calendario dentro del ciclo contable del conjunto. Se utiliza para organizar los asientos y controlar el cierre mensual.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Año Fiscal (`fiscalYear`)** | `int` | Sí | Año calendario. Ej. `2026`. |
| **Mes (`month`)** | `int` | Sí | Número del mes (1-12). |
| **Etiqueta (`periodLabel`)** | `string` max 20 | Automático | Nombre del período. Ej. "2026-06". |
| **Estado (`status`)** | `Enum` | Sí | `Open` = Abierto para contabilizar · `Closed` = Cerrado, no se pueden registrar asientos. |
| **Fecha de Apertura (`openedAt`)** | `datetime` | Automático | Fecha en que se abrió el período. |
| **Fecha de Cierre (`closedAt`)** | `datetime` | No | Fecha en que se cerró el período. |
| **Cerrado Por (`closedByUserId`)** | `string` FK | No | ID del usuario que ejecutó el cierre. |
| **Último Número de Asiento (`lastEntryNumber`)** | `int` | Automático | Contador del último número de asiento asignado en el período. Se incrementa automáticamente. |

### 4.6 Cuentas Bancarias (`BankAccount`)
Registro de las cuentas bancarias del conjunto, asociadas a la cuenta contable `1110` (Bancos) del PUC.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Cuenta Contable (`accountingAccountId`)** | `Guid` FK | Sí | Cuenta del PUC asociada (generalmente una subcuenta de `1110`). |
| **Nombre del Banco (`bankName`)** | `string` max 200 | Sí | Nombre de la entidad bancaria. Ej. "Bancolombia", "Davivienda". |
| **Número de Cuenta (`accountNumber`)** | `string` max 50 | Sí | Número de la cuenta bancaria. Único por tenant. |
| **Tipo de Cuenta (`accountType`)** | `Enum` | Sí | `Checking` = Cuenta corriente · `Savings` = Cuenta de ahorros. |
| **Saldo Actual (`currentBalance`)** | `decimal(18,2)` | Automático | Saldo según libro mayor del ERP. Se actualiza con cada movimiento registrado. |
| **Saldo Inicial (`openingBalance`)** | `decimal(18,2)` | Sí | Saldo al momento de crear la cuenta en el sistema. |
| **Activa (`isActive`)** | `boolean` | Sí | `false` si la cuenta fue cerrada o dejó de usarse. |

### 4.7 Movimientos Bancarios (`BankMovement`)
Registro individual de cada transacción que afecta el saldo de una cuenta bancaria.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Cuenta Bancaria (`bankAccountId`)** | `Guid` FK | Sí | Cuenta bancaria afectada. |
| **Asiento Contable (`accountingEntryId`)** | `Guid` FK | No | Asiento contable asociado al movimiento (si aplica). |
| **Tipo de Movimiento (`movementType`)** | `Enum` | Sí | `Deposit` = Consignación · `Withdrawal` = Retiro · `Transfer` = Transferencia · `Fee` = Comisión · `Interest` = Rendimiento. |
| **Monto (`amount`)** | `decimal(18,2)` | Sí | Valor del movimiento. Positivo para ingresos, negativo para egresos. |
| **Fecha del Movimiento (`movementDate`)** | `datetime` | Sí | Fecha en que ocurrió la transacción bancaria. |
| **Descripción (`description`)** | `string` max 500 | Sí | Concepto del movimiento. |
| **Número de Referencia (`referenceNumber`)** | `string` max 100 | No | Número de cheque, consignación o transacción. |
| **Saldo Parcial (`runningBalance`)** | `decimal(18,2)` | Automático | Saldo de la cuenta después de aplicar este movimiento. |
| **Creado Por (`createdByUserId`)** | `string` FK | Automático | ID del usuario que registró el movimiento. |

### 4.8 Conciliaciones Bancarias (`BankReconciliation`)
Proceso de verificación mensual que compara el saldo contable (ERP) contra el saldo del extracto bancario.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Cuenta Bancaria (`bankAccountId`)** | `Guid` FK | Sí | Cuenta bancaria que se concilia. |
| **Año Fiscal (`fiscalYear`)** | `int` | Sí | Año de la conciliación. |
| **Mes (`month`)** | `int` | Sí | Mes de la conciliación. Solo una conciliación por cuenta/mes. |
| **Etiqueta (`periodLabel`)** | `string` max 20 | Automático | Ej. "2026-06". |
| **Saldo en Libros (`bookBalance`)** | `decimal(18,2)` | Automático | Saldo según el ERP al cierre del mes. |
| **Saldo del Extracto (`statementBalance`)** | `decimal(18,2)` | Sí | Saldo reportado por el banco en el extracto. |
| **Diferencia (`difference`)** | `decimal(18,2)` | Automático | `statementBalance - bookBalance`. Debe ser 0 después de conciliar. |
| **Estado (`status`)** | `Enum` | Sí | `InProgress` = En proceso · `Completed` = Conciliada. |
| **Creado Por (`createdByUserId`)** | `string` FK | Automático | ID del usuario que inició la conciliación. |
| **Completado Por (`completedByUserId`)** | `string` FK | No | ID del usuario que completó la conciliación. |

### 4.9 Ítems de Conciliación (`ReconciliationItem`)
Cada partida individual que se concilia entre los libros contables y el extracto bancario.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Conciliación Bancaria (`bankReconciliationId`)** | `Guid` FK | Sí | Conciliación a la que pertenece el ítem. |
| **Movimiento Bancario (`bankMovementId`)** | `Guid` FK | No | Movimiento del ERP asociado (si existe en libros). |
| **Descripción (`description`)** | `string` max 500 | Sí | Concepto de la partida. |
| **Monto (`amount`)** | `decimal(18,2)` | Sí | Valor de la partida. |
| **Fecha del Movimiento (`movementDate`)** | `datetime` | Sí | Fecha de la transacción. |
| **Está en Libros (`isInBooks`)** | `boolean` | Sí | `true` si la partida aparece en el ERP. |
| **Está en Extracto (`isInStatement`)** | `boolean` | Sí | `true` si la partida aparece en el extracto bancario. |
| **Está Conciliada (`isCleared`)** | `boolean` | Sí | `true` cuando la partida ha sido verificada en ambos lados. |

### 4.10 Activos Fijos (`FixedAsset`)
Registro de bienes muebles e inmuebles propiedad del conjunto que se deprecian periódicamente según su vida útil.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Cuenta Contable (`accountingAccountId`)** | `Guid` FK | No | Cuenta del activo en el PUC (ej. `1520` Maquinaria y Equipo). |
| **Nombre (`name`)** | `string` max 200 | Sí | Nombre del activo. Ej. "Bomba de agua sistema presión". |
| **Descripción (`description`)** | `string` max 1000 | No | Detalle adicional del activo. |
| **Número de Serie (`serialNumber`)** | `string` max 100 | No | Número de serie o placa del fabricante. |
| **Ubicación (`location`)** | `string` max 200 | No | Lugar físico donde se encuentra el activo. |
| **Valor de Adquisición (`acquisitionValue`)** | `decimal(18,2)` | Sí | Costo de compra del activo, incluyendo impuestos no recuperables. |
| **Fecha de Adquisición (`acquisitionDate`)** | `datetime` | Sí | Fecha de compra o puesta en servicio. |
| **Vida Útil en Meses (`usefulLifeMonths`)** | `int` | Sí | Meses durante los cuales se depreciará el activo. |
| **Valor Residual (`residualValue`)** | `decimal(18,2)` | Sí | Valor estimado al final de la vida útil. |
| **Método de Depreciación (`depreciationMethod`)** | `Enum` | Sí | `StraightLine` = Línea recta (único método soportado actualmente). |
| **Depreciación Acumulada (`accumulatedDepreciation`)** | `decimal(18,2)` | Automático | Suma de todas las depreciaciones mensuales registradas. |
| **Valor en Libros (`bookValue`)** | `decimal(18,2)` | Automático | `acquisitionValue - accumulatedDepreciation`. |
| **Estado (`status`)** | `Enum` | Sí | `Active` = En uso · `Disposed` = Dado de baja · `FullyDepreciated` = Totalmente depreciado. |
| **Fecha de Baja (`disposalDate`)** | `datetime` | No | Fecha en que se dio de baja el activo. |
| **Valor de Baja (`disposalValue`)** | `decimal(18,2)` | No | Valor recibido por la venta o desecho. |
| **Motivo de Baja (`disposalReason`)** | `string` max 500 | No | Razón de la baja (venta, pérdida, donación). |

### 4.11 Depreciación Mensual (`MonthlyDepreciation`)
Registro mensual del cargo por depreciación de cada activo fijo.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Activo Fijo (`fixedAssetId`)** | `Guid` FK | Sí | Activo que se deprecia. |
| **Asiento Contable (`accountingEntryId`)** | `Guid` FK | No | Asiento generado por la depreciación (Débito Gasto / Crédito Depreciación Acumulada). |
| **Año Fiscal (`fiscalYear`)** | `int` | Sí | Año de la depreciación. |
| **Mes (`month`)** | `int` | Sí | Mes de la depreciación. |
| **Etiqueta (`periodLabel`)** | `string` max 20 | Automático | Ej. "2026-06". |
| **Monto de Depreciación (`depreciationAmount`)** | `decimal(18,2)` | Automático | Valor calculado según el método de depreciación del activo. |
| **Depreciación Acumulada Después (`accumulatedAfter`)** | `decimal(18,2)` | Automático | Depreciación acumulada después de aplicar este mes. |
| **Valor en Libros Después (`bookValueAfter`)** | `decimal(18,2)` | Automático | Valor contable después de la depreciación del mes. |

### 4.12 Presupuesto Anual (`Budget`)
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

### 4.13 Detalle del Presupuesto (`BudgetDetail`)
Asignación de valor aprobado a cada cuenta de ingreso o gasto dentro de un presupuesto.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Presupuesto (`budgetId`)** | `Guid` FK | Sí | Presupuesto al que pertenece este rubro. |
| **Cuenta Contable (`accountingAccountId`)** | `Guid` FK | Sí | Debe ser una cuenta de movimiento (`isGroup = false`) de categoría `Income` o `Expense`. Una misma cuenta solo puede aparecer una vez por presupuesto (índice único). |
| **Valor Aprobado (`approvedValue`)** | `decimal(18,2)` | Sí | Monto que la asamblea aprobó para esta cuenta en el período fiscal. Solo afectable después de la activación mediante movimientos presupuestales. |
| **Observaciones (`observations`)** | `string` max 500 | No | Notas sobre el criterio utilizado para definir el valor de este rubro. |

### 4.14 Movimientos Presupuestales (`BudgetMovement`)
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

### 4.15 Fondo de Imprevistos (`ContingencyFund`)
Reserva obligatoria según el **Artículo 35 de la Ley 675 de 2001**. Se constituye con un porcentaje mínimo del 1% de los ingresos del período y solo puede usarse para expensas imprevistas o de urgencia con aprobación del consejo.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Saldo Actual (`currentBalance`)** | `decimal(18,2)` | Automático | Calculado por el sistema. Se incrementa con cada aporte mensual y se reduce con cada uso aprobado. No editable manualmente. |

> [!IMPORTANT]
> **Tope de acumulación**: El saldo del fondo de imprevistos no puede superar el **10% del presupuesto anual** vigente. Si al intentar realizar un aporte mensual se supera este límite, el sistema **no genera el aporte** y lo omite automáticamente. Esta regla evita la acumulación excesiva de recursos en el fondo más allá de lo razonable para gastos imprevistos.

### 4.16 Aportes al Fondo de Imprevistos (`ContingencyFundContribution`)
Registro de cada aporte mensual liquidado al fondo. El sistema genera automáticamente el asiento contable correspondiente (Débito 5196 / Crédito 3205).

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Período (`period`)** | `string` max 7 | Sí | Formato `YYYY-MM`. Ej. `2025-06`. No puede liquidarse dos veces el mismo período (índice único por tenant + período). |
| **Monto Aportado (`amount`)** | `decimal(18,2)` | Automático | Calculado como `incomeBase × (percentage / 100)`. |
| **Base de Ingresos (`incomeBase`)** | `decimal(18,2)` | Automático | Suma de todos los asientos crédito menos débito en cuentas de categoría `Income` durante el período. |
| **Porcentaje Aplicado (`percentage`)** | `decimal(5,2)` | Automático | Porcentaje vigente configurado en la cuenta del conjunto al momento de la liquidación. |
| **Fecha de Liquidación (`contributionDate`)** | `datetime` | Automático | Fecha y hora en que se ejecutó la liquidación mensual. |
| **Referencia Asiento (`accountingRecordId`)** | `Guid` FK | Automático | Referencia al asiento contable de gasto (Débito 5196) generado por la liquidación. |

### 4.17 Usos del Fondo de Imprevistos (`ContingencyFundUsage`)
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
| **Cuota Ordinaria (`unitFeeId`)** | `Guid?` FK | No | ID de la cuota ordinaria asociada (si el interés proviene de una cuota ordinaria). Nullable para soportar intereses de extraordinarias o individuales. |
| **Distribución Extraordinaria (`extraordinaryFeeDistributionId`)** | `Guid?` FK | No | ID de la distribución de cuota extraordinaria asociada (si el interés proviene de una extraordinaria). |
| **Cobro Individual (`individualChargeId`)** | `Guid?` FK | No | ID del cobro individual asociado (si el interés proviene de una multa o daño). |
| **Período (`period`)** | `string` max 7 | Sí | Período en que se calculó el interés. Formato `YYYY-MM`. |
| **Monto Base (`baseAmount`)** | `decimal(18,2)` | Sí | Saldo de capital sobre el que se calculó el interés. |
| **Tasa Diaria (`dailyRate`)** | `decimal(12,8)` | Sí | `tasaMensual / 30 / 100`. La tasa mensual se toma de `TenantConfiguration.LatePaymentInterestRate`. |
| **Días de Mora (`daysOverdue`)** | `int` | Sí | Número de días entre la fecha de vencimiento y la fecha de cálculo. |
| **Monto Calculado (`calculatedAmount`)** | `decimal(18,2)` | Sí | `baseAmount × dailyRate × daysOverdue`. Redondeado a 2 decimales. |
| **Está Capitalizado (`isCapitalized`)** | `boolean` | Sí | `true` cuando el interés ha sido formalmente incorporado al capital para efectos de cobro judicial. |

> [!NOTE]
> Los campos `unitFeeId`, `extraordinaryFeeDistributionId` e `individualChargeId` reemplazan al anterior par `sourceType`/`sourceId`. Ahora se usan FK directas nullable para una mejor integridad referencial. Un registro de interés puede estar asociado a **una sola** de estas tres entidades como máximo.

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

### 5.12 Deuda de Acuerdo de Pago (`AgreementDebt`)
Vincula un acuerdo de pago con las deudas subyacentes (cuotas ordinarias, extraordinarias, cobros individuales) que fueron incluidas en el acuerdo. Permite rastrear qué obligaciones específicas cubre cada acuerdo.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Acuerdo de Pago (`paymentAgreementId`)** | `Guid` FK | Sí | Acuerdo al que pertenece esta deuda vinculada. |
| **Tipo de Origen (`sourceType`)** | `string` max 30 | Sí | `UnitFee`, `ExtraordinaryFeeDistribution` o `IndividualCharge`. |
| **ID de Origen (`sourceId`)** | `Guid` | Sí | ID del registro de la deuda original (cuota, distribución o cobro). |
| **Saldo Original (`originalBalance`)** | `decimal(18,2)` | Sí | Monto de la deuda al momento de incluirse en el acuerdo. |
| **Fecha de Creación (`createdAt`)** | `datetime` | Automático | Fecha y hora en que se vinculó la deuda al acuerdo. |

> [!NOTE]
> Índice único por `(PaymentAgreementId, SourceType, SourceId)` para evitar duplicados. Al crear el acuerdo, las obligaciones incluidas se marcan automáticamente como cubiertas por el acuerdo.

---

## 6. Módulo de Notificaciones

### 6.1 Notificación (`Notification`)
Notificaciones in-app dirigidas a propietarios. Se generan automáticamente por eventos del sistema (ej. transferencia de propiedad, vencimiento de cuotas).

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Propietario (`ownerId`)** | `Guid` FK | Sí | Propietario destinatario de la notificación. |
| **Título (`title`)** | `string` max 200 | Sí | Título breve de la notificación. |
| **Mensaje (`message`)** | `string` max 2000 | Sí | Cuerpo del mensaje de la notificación. |
| **Leída (`isRead`)** | `boolean` | Sí | `false` por defecto. El propietario la marca como leída desde la app. |
| **Fecha de Creación (`createdAt`)** | `datetime` | Automático | Fecha y hora de generación de la notificación. |

> [!NOTE]
> Índice compuesto por `(OwnerId, IsRead, CreatedAt)` para consultas eficientes de bandeja de notificaciones. Las notificaciones se crean con `isRead = false` y el frontend las ordena por fecha descendente.

---

## 7. Módulo de Caché de Indicadores

### 7.1 Caché de Indicador (`IndicatorCache`)
Caché persistente para indicadores del dashboard y reportes. Almacena resultados de cálculos costosos (mora total, recaudo del mes, indicadores de cartera) para evitar recalcularlos en cada petición.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Clave (`cacheKey`)** | `string` max 200 | Sí | Identificador único del indicador dentro del tenant. Ej. `"mora_map"`, `"portfolio_summary"`. |
| **Valor (`cacheValue`)** | `longtext` | Sí | Valor serializado del indicador (JSON). |
| **Estado (`status`)** | `Enum` | Sí | `Valid` = Dato vigente · `Invalid` = Requiere recálculo. Almacenado como string. |
| **Última Actualización (`lastUpdatedAt`)** | `datetime` | Automático | Fecha y hora del último cálculo. |
| **Próxima Invalidación (`nextInvalidationAt`)** | `datetime` | No | Fecha programada para invalidación automática (si aplica). |
| **Conteo de Consultas (`hitCount`)** | `int` | Automático | Número de veces que se ha consultado este indicador desde su último cálculo. |
| **Conteo de Invalidaciones (`invalidationCount`)** | `int` | Automático | Número de veces que se ha invalidado este indicador. |

> [!IMPORTANT]
> El sistema invalida automáticamente la caché cuando ocurren eventos que afectan los indicadores: creación/actualización de pagos, cuotas, unidades, acuerdos de pago, etc. La próxima consulta al dashboard dispara el recálculo bajo demanda. Índice único por `(TenantId, CacheKey)`.

---

## 8. Módulo PQR (Peticiones, Quejas y Reclamos)

### 8.1 PQR (`PqrRecord`)
Solicitud formal dirigida a la administración del conjunto. Clasificada en Petición, Queja o Reclamo con ciclo de vida completo.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Número de Radicado (`radicadoNumber`)** | `string` max 30 | Automático | Formato `PQR-YYYY-MM-NNNNN`. Único por tenant. Generado automáticamente al radicar. |
| **Tipo (`pqrType`)** | `Enum` string | Sí | `Request` (Petición) · `Complaint` (Queja) · `Claim` (Reclamo). |
| **Categoría (`category`)** | `Enum` string | Sí | `Billing` · `Maintenance` · `Coexistence` · `CommonAreas` · `Administration` · `Other`. |
| **Estado (`status`)** | `Enum` string | Sí | `Filed` · `UnderReview` · `InManagement` · `Responded` · `Closed` · `Reopened` · `Escalated`. |
| **Prioridad (`priority`)** | `Enum` string | Sí | `Low` · `Normal` · `High` · `Urgent`. Por defecto `Normal`. |
| **Asunto (`subject`)** | `string` max 300 | Sí | Título breve de la solicitud. |
| **Descripción (`description`)** | `string` max 4000 | Sí | Texto libre con el detalle de la solicitud. |
| **Nombre del Radicante (`radiadorName`)** | `string` max 300 | Sí | Nombre de la persona que radica la PQR. |
| **Tipo Documento Radicante (`radiadorDocumentType`)** | `string` max 20 | No | CC, NIT, CE, Pasaporte, etc. |
| **Documento Radicante (`radiadorDocumentNumber`)** | `string` max 50 | No | Número de identificación del radicante. |
| **Contacto Radicante (`radiadorContact`)** | `string` max 200 | No | Correo o teléfono del radicante. |
| **Propietario (`ownerId`)** | `Guid?` FK | No | Referencia al propietario si está registrado. |
| **Arrendatario (`tenantResidentId`)** | `Guid?` FK | No | Referencia al arrendatario si está registrado. |
| **Unidad (`unitId`)** | `Guid` FK | Sí | Unidad desde la cual se radica la PQR. |
| **Canal (`channel`)** | `Enum` string | Sí | `InPerson` · `Email` · `Phone` · `Web` · `WhatsApp` · `Letter` · `Other`. |
| **PQR Relacionada (`relatedPQRId`)** | `Guid?` FK | No | Referencia a otra PQR anterior relacionada. |
| **Asignado A (`assignedToUserId`)** | `string` max 450 | No | ID del usuario interno responsable de atender la PQR. |
| **Fecha Límite (`deadline`)** | `datetime` | No | Fecha límite de respuesta calculada en días hábiles según configuración. |
| **Residente Involucrado (`involvedResidentName`)** | `string` max 300 | No | Para quejas: nombre del residente involucrado en el conflicto (confidencial). |
| **Unidad Involucrada (`involvedResidentUnitId`)** | `Guid?` FK | No | Para quejas: unidad del residente involucrado. |
| **Es Interna (`isInternal`)** | `boolean` | Sí | `true` si fue generada por la administración (no visible para residentes). |
| **Vinculada a Cobro (`isLinkedToCharge`)** | `boolean` | Sí | `true` si el reclamo está vinculado a una cuota ordinaria, extraordinaria o cobro individual. |
| **Cuota Ordinaria (`unitFeeId`)** | `Guid?` FK | No | Cuota ordinaria asociada al reclamo. |
| **Distribución Extraordinaria (`extraordinaryFeeDistributionId`)** | `Guid?` FK | No | Distribución de cuota extraordinaria asociada. |
| **Cobro Individual (`individualChargeId`)** | `Guid?` FK | No | Cobro individual asociado al reclamo. |
| **Reclamo Resuelto (`claimResolved`)** | `boolean?` | No | `true` si el reclamo fue declarado procedente, `false` si improcedente. |
| **Nota de Resolución (`claimResolutionNote`)** | `string` max 2000 | No | Justificación de la resolución del reclamo. |
| **Nota de Crédito Generada (`creditNoteGenerated`)** | `boolean` | Sí | `true` si se generó automáticamente un ajuste en el módulo de cuotas. |
| **Fecha de Radicación (`filedAt`)** | `datetime` | Automático | Fecha y hora de radicación. |
| **Fecha de Cierre (`closedAt`)** | `datetime` | No | Fecha en que se cerró la PQR. |
| **Cierre Definitivo (`closedDefinitivelyAt`)** | `datetime` | No | Fecha después de la cual no se puede reabrir la PQR (10 días después del cierre). |

### 8.2 Seguimiento de PQR (`PqrFollowUp`)
Registro de cada cambio de estado de una PQR.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **PQR (`pqrId`)** | `Guid` FK | Sí | PQR asociada al seguimiento. |
| **Estado Anterior (`previousStatus`)** | `Enum` string | Sí | Estado antes del cambio. |
| **Estado Nuevo (`newStatus`)** | `Enum` string | Sí | Estado después del cambio. |
| **Fecha del Cambio (`changedAt`)** | `datetime` | Automático | Fecha y hora del cambio. |
| **Usuario (`changedByUserId`)** | `string` max 450 | Sí | ID del usuario que realizó el cambio. |
| **Nombre del Usuario (`changedByUserName`)** | `string` max 300 | Sí | Nombre visible del usuario. |
| **Justificación (`justification`)** | `string` max 2000 | Sí | Motivo del cambio de estado. |
| **Es Automático (`isAutomatic`)** | `boolean` | Sí | `true` si el cambio fue generado por el sistema (alertas, vencimientos). |

### 8.3 Respuesta PQR (`PqrResponse`)
Respuesta formal emitida por la administración al radicante.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **PQR (`pqrId`)** | `Guid` FK | Sí | PQR a la que pertenece la respuesta. |
| **Texto de Respuesta (`responseText`)** | `string` max 4000 | Sí | Contenido de la respuesta. |
| **Es Definitiva (`isDefinitive`)** | `boolean` | Sí | `true` si la respuesta es definitiva y cierra la PQR. |
| **Es Parcial (`isPartialUpdate`)** | `boolean` | Sí | `true` si es una actualización parcial del estado. |
| **Fecha de Envío (`sentAt`)** | `datetime` | Automático | Fecha y hora de envío. |
| **Requiere Confirmación (`requiresConfirmation`)** | `boolean` | Sí | `true` si se requiere que el radicante confirme recepción. |
| **Confirmado Por Radicante (`confirmedByRadiador`)** | `boolean?` | No | `true` si el radicante confirmó la respuesta. |
| **Fecha de Confirmación (`confirmedAt`)** | `datetime` | No | Fecha en que el radicante confirmó. |

### 8.4 Nota Interna PQR (`PqrInternalNote`)
Notas visibles exclusivamente para el equipo de administración. Nunca expuestas al residente.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **PQR (`pqrId`)** | `Guid` FK | Sí | PQR asociada. |
| **Texto (`noteText`)** | `string` max 4000 | Sí | Contenido de la nota. |
| **Autor (`authorName`)** | `string` max 300 | Sí | Nombre del autor. |
| **Usuario (`createdByUserId`)** | `string` max 450 | Sí | ID del usuario que creó la nota. |

> [!WARNING]
> **Restricción de visibilidad**: Estas notas nunca deben incluirse en respuestas a endpoints accesibles por residentes. En el endpoint `GET /api/pqr/{id}` se filtran por rol del usuario autenticado.

### 8.5 Archivo PQR (`PqrFile`)
Archivos adjuntos asociados a una PQR, sus respuestas o notas internas.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **PQR (`pqrId`)** | `Guid` FK | Sí | PQR propietaria del archivo. |
| **Respuesta (`pqrResponseId`)** | `Guid?` FK | No | Respuesta a la que está adjunto (si aplica). |
| **Nota Interna (`pqrInternalNoteId`)** | `Guid?` FK | No | Nota interna a la que está adjunto (si aplica). |
| **Nombre Interno (`fileName`)** | `string` max 500 | Sí | Nombre único con el que se almacena. |
| **Nombre Original (`originalFileName`)** | `string` max 500 | Sí | Nombre original del archivo subido. |
| **Tipo de Contenido (`contentType`)** | `string` max 200 | Sí | MIME type del archivo. |
| **Tamaño (`fileSize`)** | `long` | Sí | Tamaño en bytes. |
| **Ruta (`filePath`)** | `string` max 1000 | Sí | Ruta física de almacenamiento. |
| **Subido Por (`uploadedByUserId`)** | `string` max 450 | Sí | ID del usuario que subió el archivo. |
| **Nombre del Usuario (`uploadedByUserName`)** | `string` max 300 | Sí | Nombre visible. |
| **Es del Radicante (`isFromApplicant`)** | `boolean` | Sí | `true` si fue subido por el radicante. |

### 8.6 Configuración de Tiempos PQR (`PqrTimeConfig`)
Tiempos límite configurables por el administrador para cada tipo de PQR.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Tipo (`pqrType`)** | `Enum` string | Sí | `Request` · `Complaint` · `Claim`. Índice único por tenant+tipo. |
| **Días Hábiles (`businessDays`)** | `int` | Sí | Días hábiles para respuesta. Por defecto: Petición=5, Queja=3, Reclamo=10. |

### 8.7 Alerta PQR (`PqrAlert`)
Alertas generadas automáticamente por el motor de vencimiento de tiempos.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **PQR (`pqrId`)** | `Guid` FK | Sí | PQR que generó la alerta. |
| **Tipo (`alertType`)** | `Enum` string | Sí | `FiftyPercent` (50% plazo) · `EightyPercent` (80% plazo) · `Overdue` (vencida). |
| **Generada El (`generatedAt`)** | `datetime` | Automático | Fecha de generación. |
| **Activa (`isActive`)** | `boolean` | Sí | `true` mientras no haya sido resuelta. |
| **Resuelta El (`resolvedAt`)** | `datetime` | No | Fecha en que se resolvió la alerta. |
| **Escalada al Consejo (`escalatedToCouncil`)** | `boolean` | Sí | `true` si la alerta fue escalada al Consejo de Administración. |

### 8.8 Reglas de Negocio del Módulo PQR

> [!IMPORTANT]
> **Radicación**: El número de radicado se genera automáticamente con formato `PQR-YYYY-MM-NNNNN` y no puede modificarse.
>
> **Fecha límite**: Se calcula en días hábiles (lunes a viernes) al momento de radicar según la configuración del tenant para cada tipo de PQR.
>
> **Alertas automáticas**: El motor de alertas (`PQRAlertEngineService`) ejecuta cada 15 minutos:
> - Al **50%** del plazo sin cambio de estado: alerta interna al administrador.
> - Al **80%** del plazo sin respuesta: alerta escalada al Consejo de Administración.
> - Al **100%** (vencimiento): la PQR se marca automáticamente como `Escalated` y se genera alerta crítica.
>
> **Vinculación con cartera**: Los reclamos sobre cobros pueden vincularse a `UnitFee`, `ExtraordinaryFeeDistribution` o `IndividualCharge`. Si el reclamo es declarado procedente, se genera una nota de crédito.
>
> **Reapertura**: Una PQR cerrada puede ser reabierta dentro de los 10 días siguientes al cierre. Después de ese plazo, queda cerrada definitivamente.
>
> **Confidencialidad**: Las quejas que involucran a otro residente registran su nombre y unidad, pero este no recibe notificaciones sobre el contenido de la queja.
>
> **Inmutabilidad del historial**: El historial completo (seguimientos, respuestas, notas internas, archivos) se conserva indefinidamente y no puede ser eliminado por ningún usuario.

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
| `erp_entry_lines` | Contabilidad | Líneas de detalle de cada asiento |
| `erp_entry_reversals` | Contabilidad | Reversiones de asientos contables |
| `erp_accounting_periods` | Contabilidad | Períodos contables mensuales |
| `erp_bank_accounts` | Contabilidad | Cuentas bancarias del conjunto |
| `erp_bank_movements` | Contabilidad | Movimientos registrados en cuentas bancarias |
| `erp_bank_reconciliations` | Contabilidad | Conciliaciones bancarias mensuales |
| `erp_reconciliation_items` | Contabilidad | Partidas individuales de conciliación |
| `erp_fixed_assets` | Contabilidad | Activos fijos del conjunto |
| `erp_monthly_depreciations` | Contabilidad | Depreciación mensual de activos fijos |
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
| `erp_agreement_debts` | Cuotas y Cartera | Deudas subyacentes vinculadas a acuerdos de pago |
| `erp_clearance_certificates` | Cuotas y Cartera | Paz y salvos expedidos |
| `erp_configuration_audit_logs` | Configuración | Auditoría de cambios en parámetros del conjunto |
| `erp_notifications` | Notificaciones | Notificaciones in-app para propietarios |
| `erp_indicator_caches` | Caché | Caché persistente de indicadores del dashboard |
| `erp_pqr_records` | PQR | Solicitudes formales (Peticiones, Quejas, Reclamos) |
| `erp_pqr_follow_ups` | PQR | Historial de cambios de estado de PQR |
| `erp_pqr_responses` | PQR | Respuestas emitidas al radicante |
| `erp_pqr_internal_notes` | PQR | Notas internas del equipo de administración (no visibles para residentes) |
| `erp_pqr_files` | PQR | Archivos adjuntos de PQR, respuestas y notas internas |
| `erp_pqr_time_configs` | PQR | Configuración de días hábiles por tipo de PQR |
| `erp_pqr_alerts` | PQR | Alertas generadas por vencimiento de tiempos |

---

## 9. Estándar de Campos en Frontend

Esta sección define el estándar visual y de validación para todos los formularios del sistema.

### 9.1 Estilo de Inputs de Texto y Selects

Todos los campos de texto y selects deben usar la siguiente clase base:

```
w-full bg-transparent border-b border-emerald-600/30 focus:border-emerald-600 text-sm font-medium py-2 outline-none
```

- **`border-b border-emerald-600/30`**: Borde inferior tenue (30% opacidad) para un diseño limpio.
- **`focus:border-emerald-600`**: Al enfocar, el borde inferior se vuelve sólido.
- **`text-sm font-medium`**: Texto pequeño con peso medio.
- **`py-2`**: Espaciado vertical estándar.
- **`outline-none`**: Sin contorno nativo del navegador.

### 9.2 Estilo de Textareas

```
w-full bg-slate-50 dark:bg-slate-900 border border-border focus:border-emerald-600 rounded-md text-sm p-3 outline-none resize-none
```

### 9.3 Etiquetas

```
block text-xs font-bold text-muted-foreground uppercase tracking-widest mb-1.5
```

- Texto pequeño (`text-xs`), bold, mayúsculas sostenidas, color secundario.

### 9.4 Restricciones por Tipo de Campo

| Tipo de Campo | Atributos HTML | Uso |
|---------------|----------------|-----|
| **Texto corto** (nombre, asunto) | `maxLength={200}` | Nombres, asuntos breves |
| **Texto mediano** (asunto largo) | `maxLength={500}` | Asuntos de PQR, títulos |
| **Texto largo** (descripción) | `maxLength={4000}` | Cuerpo de descripciones |
| **Número de documento** | `maxLength={20}` | IDs, números de documento |
| **Contacto** (tel/email) | `maxLength={200}` | Correos, teléfonos |
| **Select obligatorio** | `required` + option vacío con "Seleccione..." | Cuando el usuario debe escoger un valor |
| **Checkbox** | `className="accent-emerald-600 w-5 h-5"` | Palancas de opción binaria |

### 9.5 Botones

| Variante | Clase | Uso |
|----------|-------|-----|
| Primario | `<Button variant="primary">` | Acción principal del formulario |
| Secundario | `<Button variant="secondary">` | Acción secundaria (ej. simular) |
| Ghost | `<Button variant="ghost">` | Cancelar, acciones de baja jerarquía |

### 9.6 Validación en Formularios

1. **Validación cliente**: Siempre validar campos obligatorios antes de enviar con `if (!campo) { setError('Mensaje'); return; }`.
2. **Errores**: Mostrar en un contenedor `bg-rose-50 border border-rose-200 rounded-lg text-rose-700 text-xs flex items-center gap-2` con icono `AlertTriangle`.
3. **Carga**: Usar `<Loader2 className="w-6 h-6 animate-spin text-emerald-600" />` centrado.
4. **Deshabilitado**: Botón de submit debe mostrar `disabled={submitting}` y spinner mientras se envía.
