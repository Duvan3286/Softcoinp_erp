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
| `erp_communications` | Comunicaciones | Comunicados formales enviados por la administración |
| `erp_communication_recipients` | Comunicaciones | Estado de entrega individual por destinatario y canal |
| `erp_communication_preferences` | Comunicaciones | Preferencias de canales de comunicación por residente |
| `erp_notification_templates` | Comunicaciones | Plantillas configurables para notificaciones automáticas |
| `erp_automatic_notifications` | Comunicaciones | Notificaciones generadas automáticamente por eventos del sistema |
| `erp_bulletin_board_posts` | Comunicaciones | Publicaciones en la cartelera digital del portal |
| `erp_delinquency_sequence_configs` | Comunicaciones | Configuración de pasos de la secuencia de avisos de mora |
| `erp_delinquency_sequence_pauses` | Comunicaciones | Pausas de secuencia de mora por unidad |
| `erp_indicator_caches` | Caché | Caché persistente de indicadores del dashboard |
| `erp_pqr_records` | PQR | Solicitudes formales (Peticiones, Quejas, Reclamos) |
| `erp_pqr_follow_ups` | PQR | Historial de cambios de estado de PQR |
| `erp_pqr_responses` | PQR | Respuestas emitidas al radicante |
| `erp_pqr_internal_notes` | PQR | Notas internas del equipo de administración (no visibles para residentes) |
| `erp_pqr_files` | PQR | Archivos adjuntos de PQR, respuestas y notas internas |
| `erp_pqr_time_configs` | PQR | Configuración de días hábiles por tipo de PQR |
| `erp_pqr_alerts` | PQR | Alertas generadas por vencimiento de tiempos |
| `erp_contracts` | Proveedores y Contratos | Contratos con proveedores |
| `erp_contract_policies` | Proveedores y Contratos | Pólizas de seguro de contratos |
| `erp_contract_alerts` | Proveedores y Contratos | Alertas de vencimiento y renovación de contratos |
| `erp_provider_invoices` | Proveedores y Contratos | Facturas registradas por proveedores |
| `erp_provider_payments` | Proveedores y Contratos | Pagos realizados a proveedores |
| `erp_provider_evaluations` | Proveedores y Contratos | Evaluaciones de desempeño de proveedores |
| `erp_retention_configurations` | Proveedores y Contratos | Configuración de retenciones por tipo de servicio |
| `erp_approval_thresholds` | Proveedores y Contratos | Umbrales de aprobación por nivel (Admin/Consejo/Asamblea) |
| `erp_report_types` | Reportes | Catálogo de tipos de reporte disponibles |
| `erp_generated_reports` | Reportes | Registro de reportes generados y exportados |
| `erp_recurring_report_configs` | Reportes | Configuración de reportes programados recurrentes |
| `erp_management_report_sections` | Reportes | Secciones del generador de informes anuales |
| `erp_pdf_templates` | Reportes | Personalización visual de reportes PDF |
| `erp_assemblies` | Asambleas | Cabecera de asambleas de copropietarios |
| `erp_assembly_convocations` | Asambleas | Registro de envíos de convocatoria |
| `erp_convocation_documents` | Asambleas | Documentos adjuntos a convocatorias |
| `erp_convocation_recipients` | Asambleas | Destinatarios de convocatorias por propietario |
| `erp_assembly_attendances` | Asambleas | Registro de asistencia y representación |
| `erp_assembly_agenda_items` | Asambleas | Puntos del orden del día y resultados de votación |
| `erp_assembly_constancies` | Asambleas | Constancias y objeciones presentadas en asamblea |
| `erp_assembly_minutes` | Asambleas | Actas oficiales de asambleas |
| `erp_assembly_decision_propagations` | Asambleas | Propagación de decisiones a otros módulos |
| `erp_reservable_spaces` | Reservas | Catálogo de espacios comunes reservables |
| `erp_space_schedules` | Reservas | Horarios de operación de espacios |
| `erp_space_blocks` | Reservas | Bloqueos por mantenimiento, administrativos o emergencias |
| `erp_reservations` | Reservas | Reservas de espacios comunes realizadas por residentes |
| `erp_reservation_deposits` | Reservas | Depósitos de garantía de reservas |
| `erp_reservation_incidents` | Reservas | Incidentes reportados durante uso de espacios |
| `erp_reservation_reminders` | Reservas | Recordatorios automáticos enviados antes de la reserva |

---

## 9. Módulo de Proveedores y Contratos

Gestión integral de proveedores, contratos, facturas, pagos, evaluaciones y configuración de retenciones. Este módulo permite administrar el ciclo de vida completo de las relaciones contractuales con proveedores y contratistas del conjunto.

### 9.1 Proveedor (`Provider`)

Persona natural o jurídica que presta servicios o suministra bienes al conjunto.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Tenant (`tenantId`)** | `string` max 255 | Automático | Identificador del conjunto. Se hereda del contexto. |
| **Tipo de Proveedor (`providerType`)** | `Enum` | Sí | `Natural` = Persona Natural · `Legal` = Persona Jurídica. Determina si se muestran campos de representante legal. |
| **Tipo de Documento (`documentType`)** | `string` max 20 | Sí | Tipo de identificación del proveedor (CC, NIT, CE, Pasaporte). |
| **Número de Documento (`documentNumber`)** | `string` max 50 | Sí | Número de identificación. Único por tenant. |
| **Dígito de Verificación (`verificationDigit`)** | `string` max 2 | No | Solo aplica para NIT. Se calcula con módulo 11. |
| **Razón Social (`businessName`)** | `string` max 300 | Sí | Nombre completo o razón social del proveedor. |
| **Nombre Comercial (`tradeName`)** | `string` max 300 | No | Nombre comercial o nombre corto. |
| **Nombre del Contacto (`contactName`)** | `string` max 300 | No | Nombre de la persona de contacto principal. |
| **Correo Electrónico (`email`)** | `string` max 256 | No | Correo de contacto del proveedor. |
| **Teléfono (`phone`)** | `string` max 20 | No | Teléfono de contacto. |
| **Dirección (`address`)** | `string` max 500 | No | Dirección física del proveedor. |
| **Ciudad (`city`)** | `string` max 100 | No | Ciudad de residencia o sede. |
| **Actividad Económica (`economicActivity`)** | `string` max 200 | No | Descripción de la actividad económica principal. |
| **Tipo de Servicio (`serviceType`)** | `string` max 100 | No | Categoría del servicio que presta. Ej. "Mantenimiento", "Aseo", "Seguridad". |
| **Archivo RUT (`rutFilePath`)** | `string` max 1000 | No | Ruta del archivo del RUT escaneado. |
| **Tipo Doc. Rep. Legal (`legalRepDocumentType`)** | `string` max 20 | Cond. | Solo si `providerType = Legal`. Tipo de documento del representante. |
| **Nro. Doc. Rep. Legal (`legalRepDocumentNumber`)** | `string` max 50 | Cond. | Solo si `providerType = Legal`. Número de documento del representante. |
| **Nombre Rep. Legal (`legalRepName`)** | `string` max 300 | Cond. | Solo si `providerType = Legal`. Nombre completo del representante legal. |
| **Email Rep. Legal (`legalRepEmail`)** | `string` max 256 | Cond. | Solo si `providerType = Legal`. Correo del representante legal. |
| **Es Preferido (`isPreferred`)** | `boolean` | Sí | Marca al proveedor como preferido para búsquedas rápidas. |
| **Estado (`status`)** | `Enum` | Sí | `Active` = Activo · `Inactive` = Inactivo. Los proveedores inactivos no pueden ser asignados a nuevos contratos. |
| **Creado Por (`createdByUserId`)** | `string` max 450 | Automático | ID del usuario que registró el proveedor. |

> [!IMPORTANT]
> **Soft delete**: Los proveedores se eliminan lógicamente (`isDeleted = true`). No se borran físicamente de la base de datos para preservar la integridad referencial con contratos, facturas y evaluaciones existentes.

### 9.2 Contrato (`Contract`)

Acuerdo formal entre el conjunto y un proveedor para la prestación de servicios o suministro de bienes.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Proveedor (`providerId`)** | `Guid` FK | Sí | Proveedor asociado al contrato. No se puede cambiar después de creado. |
| **Número de Contrato (`contractNumber`)** | `string` max 50 | Sí | Código único de identificación del contrato. Único por tenant. |
| **Tipo de Contrato (`contractType`)** | `Enum` | Sí | `ServiceAgreement` = Contrato de Servicios · `Supply` = Suministro · `CivilWorks` = Obra Civil · `Lease` = Arrendamiento. |
| **Objeto del Contrato (`objectDescription`)** | `string` max 2000 | Sí | Descripción detallada del objeto, alcance y condiciones del contrato. |
| **Valor Total (`totalValue`)** | `decimal(18,2)` | Sí | Valor total del contrato en COP. |
| **Valor Mensual (`monthlyValue`)** | `decimal(18,2)` | Sí | Valor mensual del contrato (para contratos recurrentes). |
| **Es Recurrente (`isRecurrent`)** | `boolean` | Sí | Indica si el contrato genera obligaciones mensuales periódicas. |
| **Fecha de Inicio (`startDate`)** | `date` | Sí | Fecha de inicio de vigencia del contrato. |
| **Fecha de Fin (`endDate`)** | `date` | Sí | Fecha de terminación del contrato. El sistema calcula automáticamente los días restantes. |
| **Renovación Automática (`hasAutoRenewal`)** | `boolean` | Sí | Si es `true`, el sistema genera alertas antes del vencimiento para revisar la renovación. |
| **Días de Aviso Renovación (`autoRenewalNoticeDays`)** | `int` | Sí | Días de antelación para generar alerta de renovación automática. Default: 30. |
| **Nivel de Aprobación (`approvalLevel`)** | `Enum` | Sí | `Administrator` = Administrador · `Council` = Consejo de Administración · `Assembly` = Asamblea. Se determina automáticamente según los umbrales configurados. |
| **Nro. Acta Consejo (`councilMeetingActNumber`)** | `string` max 100 | Cond. | Obligatorio si `approvalLevel = Council`. Número del acta de aprobación del Consejo. |
| **Nro. Acta Asamblea (`assemblyMeetingActNumber`)** | `string` max 100 | Cond. | Obligatorio si `approvalLevel = Assembly`. Número del acta de aprobación de la Asamblea. |
| **Cuenta Presupuestal (`budgetAccountId`)** | `Guid?` FK | No | Cuenta del PUC asociada al contrato (para integración contable). |
| **Estado (`status`)** | `Enum` | Sí | `Draft` = Borrador · `Active` = Activo · `Suspended` = Suspendido · `Completed` = Completado · `Terminated` = Terminado · `Cancelled` = Cancelado. |
| **Archivo Contrato Firmado (`signedContractFilePath`)** | `string` max 1000 | No | Ruta del archivo del contrato firmado digitalmente. |
| **Creado Por (`createdByUserId`)** | `string` max 450 | Automático | ID del usuario que creó el contrato. |
| **Actualizado Por (`updatedByUserId`)** | `string` max 450 | No | ID del último usuario que modificó el contrato. |

#### Transiciones de Estado

| Estado Origen | Estados Permitidos | Requisitos |
|---------------|-------------------|------------|
| `Draft` | `Active`, `Cancelled` | Para `Active`: si aprobación es Consejo/Asamblea, debe tener número de acta. |
| `Active` | `Suspended`, `Terminated` | Requiere justificación del cambio. |
| `Suspended` | `Active`, `Terminated` | Requiere justificación para reactivar o terminar. |
| Cualquier otro | — | Estados `Completed`, `Terminated`, `Cancelled` son finales. |

### 9.3 Póliza de Seguro (`ContractPolicy`)

Registro de pólizas de seguro asociadas a un contrato (seguro de cumplimiento, seguro de vida, etc.).

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Contrato (`contractId`)** | `Guid` FK | Sí | Contrato al que pertenece la póliza. |
| **Número de Póliza (`policyNumber`)** | `string` max 100 | Sí | Número de identificación de la póliza. |
| **Aseguradora (`insuranceCompany`)** | `string` max 300 | Sí | Nombre de la empresa aseguradora. |
| **Tipo de Póliza (`policyType`)** | `string` max 100 | Sí | Tipo de cobertura. Ej. "Cumplimiento", "Responsabilidad Civil". |
| **Valor Asegurado (`insuredAmount`)** | `decimal(18,2)` | Sí | Monto asegurado en COP. |
| **Fecha de Inicio (`startDate`)** | `date` | Sí | Fecha de inicio de vigencia de la póliza. |
| **Fecha de Fin (`endDate`)** | `date` | Sí | Fecha de vencimiento de la póliza. El sistema genera alertas cuando faltan 30 días o menos. |
| **Archivo de Póliza (`filePath`)** | `string` max 1000 | No | Ruta del archivo digitalizado de la póliza. |
| **Activa (`isActive`)** | `boolean` | Sí | `false` cuando la póliza ha sido reemplazada o vencida. |
| **Creado Por (`createdByUserId`)** | `string` max 450 | Automático | ID del usuario que registró la póliza. |

### 9.4 Alerta de Contrato (`ContractAlert`)

Alertas generadas automáticamente por el motor de alertas cada 6 horas.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Contrato (`contractId`)** | `Guid` FK | Sí | Contrato asociado a la alerta. |
| **Tipo de Alerta (`alertType`)** | `Enum` | Sí | `NinetyDaysToExpiration` = Vence en 90 días · `ThirtyDaysToExpiration` = Vence en 30 días · `FifteenDaysToExpiration` = Vence en 15 días · `AutoRenewalWarning` = Renovación Automática · `PolicyExpiring` = Póliza por Vencer. |
| **Mensaje (`message`)** | `string` max 1000 | Sí | Descripción legible de la alerta. |
| **Fecha de Generación (`generatedAt`)** | `datetime` | Automático | Fecha y hora en que se generó la alerta. |
| **Activa (`isActive`)** | `boolean` | Sí | `false` cuando la alerta ha sido resuelta. |
| **Fecha de Resolución (`resolvedAt`)** | `datetime` | No | Fecha y hora en que se resolvió la alerta. |
| **Resuelta Por (`resolvedByUserId`)** | `string` max 450 | No | ID del usuario que resolvió la alerta. |
| **Escalada al Consejo (`escalatedToCouncil`)** | `boolean` | Sí | `true` cuando la alerta requiere intervención del Consejo de Administración (contratos con ≤15 días). |

### 9.5 Factura de Proveedor (`ProviderInvoice`)

Registro de facturas recibidas de proveedores asociadas a contratos.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Proveedor (`providerId`)** | `Guid` FK | Sí | Proveedor emisor de la factura. |
| **Contrato (`contractId`)** | `Guid?` FK | No | Contrato asociado (si la factura corresponde a un contrato específico). |
| **Número de Factura (`invoiceNumber`)** | `string` max 100 | Sí | Número de la factura proveedor. |
| **Fecha de Factura (`invoiceDate`)** | `date` | Sí | Fecha de emisión de la factura. |
| **Fecha de Vencimiento (`dueDate`)** | `date` | Sí | Fecha límite de pago. |
| **Subtotal (`subtotal`)** | `decimal(18,2)` | Sí | Valor base antes de impuestos. |
| **IVA (`ivaAmount`)** | `decimal(18,2)` | Sí | Valor del IVA (19%). |
| **Retención en la Fuente (`retentionFuelAmount`)** | `decimal(18,2)` | Sí | Valor de retención en la fuente calculado según la configuración del servicio. |
| **Retención ICA (`retentionIcaAmount`)** | `decimal(18,2)` | Sí | Valor de retención de ICA calculado según la configuración del servicio. |
| **Valor Neto (`netAmount`)** | `decimal(18,2)` | Sí | `subtotal + ivaAmount - retentionFuelAmount - retentionIcaAmount`. |
| **Estado (`status`)** | `Enum` | Sí | `Pending` = Pendiente · `Paid` = Pagada · `Overdue` = Vencida · `Cancelada`. |
| **Descripción (`description`)** | `string` max 2000 | No | Detalle de los servicios o productos facturados. |
| **Archivo Factura (`invoiceFilePath`)** | `string` max 1000 | No | Ruta del archivo digitalizado de la factura. |
| **Asiento Contable (`accountingEntryId`)** | `Guid?` FK | No | Referencia al asiento contable generado al contabilizar la factura. |
| **Creado Por (`createdByUserId`)** | `string` max 450 | Automático | ID del usuario que registró la factura. |
| **Actualizado Por (`updatedByUserId`)** | `string` max 450 | No | ID del último usuario que modificó la factura. |

### 9.6 Pago a Proveedor (`ProviderPayment`)

Registro de pagos realizados a proveedores para cubrir facturas.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Factura (`invoiceId`)** | `Guid` FK | Sí | Factura a la que se aplica el pago. |
| **Monto (`amount`)** | `decimal(18,2)` | Sí | Valor pagado. No puede superar el saldo pendiente de la factura. |
| **Fecha de Pago (`paymentDate`)** | `date` | Sí | Fecha en que se realizó el pago. |
| **Medio de Pago (`paymentMethod`)** | `Enum` | Sí | `Cash` = Efectivo · `BankTransfer` = Transferencia · `Check` = Cheque · `CreditCard` = Tarjeta de Crédito. |
| **Número de Referencia (`referenceNumber`)** | `string` max 100 | No | Número de consignación, cheque o transacción. |
| **Cuenta Bancaria (`bankAccount`)** | `string` max 100 | No | Cuenta bancaria desde la que se realizó el pago. |
| **Notas (`notes`)** | `string` max 1000 | No | Observaciones sobre el pago. |
| **Comprobante (`receiptFilePath`)** | `string` max 1000 | No | Ruta del comprobante de pago digitalizado. |
| **Estado (`status`)** | `Enum` | Sí | `Pending` = Pendiente · `Completed` = Completado · `Cancelled` = Cancelado. |
| **Asiento Contable (`accountingEntryId`)** | `Guid?` FK | No | Referencia al asiento contable generado al registrar el pago. |
| **Creado Por (`createdByUserId`)** | `string` max 450 | Automático | ID del usuario que registró el pago. |

### 9.7 Evaluación de Proveedor (`ProviderEvaluation`)

Evaluación periódica del desempeño de un proveedor en 4 criterios escalados del 1 al 5.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Proveedor (`providerId`)** | `Guid` FK | Sí | Proveedor evaluado. |
| **Contrato (`contractId`)** | `Guid?` FK | No | Contrato específico evaluado (si aplica). |
| **Periodo de Evaluación (`evaluationPeriod`)** | `string` max 20 | Sí | Período evaluado. Ej. "2026-Q1", "2026-S1". |
| **Calidad del Servicio (`serviceQualityScore`)** | `int` | Sí | Puntuación del 1 al 5. Evalúa la calidad técnica del servicio prestado. |
| **Cumplimiento (`complianceScore`)** | `int` | Sí | Puntuación del 1 al 5. Evalúa el cumplimiento de plazos y condiciones contractuales. |
| **Fairness del Precio (`priceFairnessScore`)** | `int` | Sí | Puntuación del 1 al 5. Evalúa la razonabilidad de los precios respecto al mercado. |
| **Post-Venta (`afterSalesScore`)** | `int` | Sí | Puntuación del 1 al 5. Evalúa la calidad del servicio post-venta y garantías. |
| **Puntaje Promedio (`averageScore`)** | `decimal(3,2)` | Automático | Promedio de los 4 criterios. Se calcula automáticamente: `(suma / 4)`. |
| **Comentarios (`comments`)** | `string` max 4000 | No | Observaciones adicionales sobre la evaluación. |
| **Recomendación (`recommendation`)** | `Enum` | Automático | `Renew` (promedio ≥ 4.0) · `EvaluateOtherOptions` (promedio ≥ 2.5) · `DoNotRenew` (promedio < 2.5). Se calcula automáticamente. |
| **Evaluado Por (`evaluatedByUserId`)** | `string` max 450 | Automático | ID del usuario que realizó la evaluación. |
| **Nombre Evaluador (`evaluatedByUserName`)** | `string` max 300 | Automático | Nombre del evaluador para display. |

### 9.8 Configuración de Retenciones (`RetentionConfiguration`)

Configuración de las tarifas de retención aplicables a cada tipo de servicio de proveedor.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Tipo de Servicio (`serviceType`)** | `string` max 100 | Sí | Categoría del servicio. Ej. "Mantenimiento", "Aseo". Único por tenant. |
| **Descripción del Servicio (`serviceDescription`)** | `string` max 500 | No | Descripción detallada del tipo de servicio. |
| **Tarifa Retención Fuente (`retentionFuelRate`)** | `decimal(5,4)` | Sí | Porcentaje de retención en la fuente. Ej. `0.0250` = 2.5%. |
| **Tarifa Retención ICA (`retentionIcaRate`)** | `decimal(5,4)` | Sí | Porcentaje de retención de ICA. Ej. `0.0028` = 0.28%. |
| **Activa (`isActive`)** | `boolean` | Sí | `false` si la configuración fue descontinuada. |
| **Creado Por (`createdByUserId`)** | `string` max 450 | Automático | ID del usuario que creó la configuración. |
| **Actualizado Por (`updatedByUserId`)** | `string` max 450 | No | ID del último usuario que modificó la configuración. |

### 9.9 Umbral de Aprobación (`ApprovalThreshold`)

Configuración de los rangos de valor para determinar qué nivel de aprobación requiere un contrato.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Nivel de Aprobación (`approvalLevel`)** | `Enum` | Sí | `Administrator` = Administrador · `Council` = Consejo · `Assembly` = Asamblea. Único por tenant. |
| **Valor Mínimo (`minValue`)** | `decimal(18,2)` | Sí | Límite inferior del rango en COP. |
| **Valor Máximo (`maxValue`)** | `decimal(18,2)` | Sí | Límite superior del rango en COP. Debe ser mayor que `minValue`. |
| **Descripción (`description`)** | `string` max 500 | No | Descripción del umbral. Ej. "Contratos menores a 10 SMLMV". |
| **Activo (`isActive`)** | `boolean` | Sí | `false` si el umbral fue descontinuado. |
| **Creado Por (`createdByUserId`)** | `string` max 450 | Automático | ID del usuario que creó el umbral. |
| **Actualizado Por (`updatedByUserId`)** | `string` max 450 | No | ID del último usuario que modificó el umbral. |

#### Lógica de Determinación del Nivel

El sistema recorre los umbrales activos ordenados de menor a mayor valor. Si el valor del contrato cae dentro de un rango, se asigna el nivel correspondiente. Si no cae en ningún rango, el nivel por defecto es `Administrator`.

### 9.10 Reglas de Negocio del Módulo

1. **Soft delete en Proveedores**: Los proveedores se eliminan lógicamente. No se pueden eliminar proveedores con contratos activos o en borrador.
2. **Contratos en Borrador**: Solo los contratos en estado `Draft` pueden editarse o eliminarse. Un contrato activo solo puede suspenderse o terminarse.
3. **Aprobación de Contratos**: El nivel de aprobación se determina automáticamente al crear el contrato según los umbrales configurados. Para activar un contrato con aprobación de Consejo o Asamblea, se requiere el número de acta correspondiente.
4. **Renovación Automática**: El motor de alertas genera avisos a los `autoRenewalNoticeDays` días del vencimiento. La renovación no es automática; requiere acción manual del administrador.
5. **Cálculo de Retenciones**: Las retenciones se calculan sobre el subtotal de la factura usando las tarifas configuradas para el tipo de servicio del contrato.
6. **Evaluaciones**: El puntaje promedio se calcula como `(calidad + cumplimiento + fairness + post-venta) / 4`. La recomendación se asigna automáticamente: ≥ 4.0 = Renovar, ≥ 2.5 = Evaluar Otras Opciones, < 2.5 = No Renovar.
7. **Motor de Alertas**: Ejecuta cada 6 horas. Genera alertas de vencimiento (90/30/15 días), pólizas por vencer (30 días), y renovación automática. Limpia alertas resueltas con más de 30 días.

---

## 10. Módulo de Mantenimiento y Zonas Comunes

Gestión del inventario físico de bienes comunes, planes de mantenimiento preventivo, órdenes de trabajo correctivo y registro de siniestros. Este módulo protege el patrimonio colectivo de los copropietarios y garantiza que las zonas comunes se conserven en condiciones óptimas.

### 10.1 Bien Común (`CommonAsset`)

Inventario físico de los bienes comunes del conjunto (ascensores, bombas de agua, piscinas, zonas verdes, etc.).

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Nombre (`name`)** | `string` max 300 | Sí | Nombre descriptivo del bien. Ej. "Ascensor Torre A", "Piscina Comunal". |
| **Categoría (`category`)** | `Enum` | Sí | `Structure` = Estructura · `ElectricalEquipment` = Equipos Eléctricos · `HydraulicEquipment` = Equipos Hidráulicos · `SafetyEquipment` = Equipos de Seguridad · `RecreationalAreas` = Zonas Recreativas · `GreenAreas` = Zonas Verdes. |
| **Ubicación (`location`)** | `string` max 300 | Sí | Ubicación física dentro del conjunto. Ej. "Torre A, Piso 1", "Área común nivel 2". |
| **Es Esencial (`isEssential`)** | `boolean` | Sí | `true` = bien cuya afectación compromete seguridad o habitabilidad (ascensores, bombas, sistemas de seguridad). `false` = bien cuya afectación reduce calidad de vida pero no compromete seguridad. Determina prioridad y nivel de aprobación para gastos. |
| **Marca (`brand`)** | `string` max 150 | No | Marca del fabricante. |
| **Modelo (`model`)** | `string` max 150 | No | Modelo del equipo. |
| **Número de Serie (`serialNumber`)** | `string` max 100 | No | Número de serie del fabricante. |
| **Fecha de Adquisición (`acquisitionDate`)** | `date` | No | Fecha en que se adquirió el bien. |
| **Valor de Adquisición (`acquisitionValue`)** | `decimal(18,2)` | No | Costo de adquisición para efectos contables. |
| **Vida Útil Estimada (`estimatedUsefulLifeMonths`)** | `int` | No | Vida útil en meses para efectos de depreciación. |
| **Proveedor de Referencia (`referenceProviderId`)** | `Guid?` FK | No | Proveedor o fabricante de referencia para mantenimiento. FK a `erp_providers`. |
| **Fabricante (`manufacturer`)** | `string` max 200 | No | Nombre del fabricante o marca de referencia. |
| **Tiene Garantía (`hasWarranty`)** | `boolean` | Sí | Indica si el bien tiene garantía vigente. |
| **Fecha Fin Garantía (`warrantyEndDate`)** | `date` | No | Fecha de vencimiento de la garantía. |
| **Estado (`status`)** | `Enum` | Sí | `Operational` = Operativo · `OperationalWithObservations` = Operativo con Observaciones · `UnderMaintenance` = En Mantenimiento · `OutOfService` = Fuera de Servicio · `Decommissioned` = Dado de Baja. |
| **Notas de Estado (`statusNotes`)** | `string` max 2000 | No | Observaciones sobre el estado actual del bien. |
| **Creado Por (`createdByUserId`)** | `string` max 450 | Automático | ID del usuario que registró el bien. |

> [!IMPORTANT]
> **Soft delete**: Los bienes se eliminan lógicamente (`isDeleted = true`). Los bienes dados de baja permanecen en el sistema con su historial pero marcados como inactivos. No se pueden eliminar bienes con órdenes de trabajo activas.

### 10.2 Fotografía del Bien (`AssetPhoto`)

Registro fotográfico del estado del bien en el tiempo. Permite seguimiento visual del deterioro o mejora.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Bien (`assetId`)** | `Guid` FK | Sí | Bien al que pertenece la fotografía. Cascade delete. |
| **Ruta del Archivo (`filePath`)** | `string` max 500 | Sí | Ruta de almacenamiento de la imagen. |
| **Descripción (`description`)** | `string` max 500 | No | Descripción de la fotografía. Ej. "Estado del ascensor antes de mantenimiento". |
| **Fecha de Captura (`capturedAt`)** | `datetime` | Automático | Fecha y hora en que se tomó la fotografía. |
| **Capturado Por (`capturedByUserId`)** | `string` max 450 | Automático | ID del usuario que subió la fotografía. |

### 10.3 Plan de Mantenimiento (`MaintenancePlan`)

Define la frecuencia y el tipo de actividad preventiva para cada bien. El motor del sistema genera órdenes de trabajo automáticamente según estos planes.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Bien (`assetId`)** | `Guid` FK | Sí | Bien al que aplica el plan. Cascade delete. |
| **Tipo de Actividad (`activityType`)** | `Enum` | Sí | `Lubrication` · `Calibration` · `Inspection` · `Cleaning` · `FilterReplacement` · `OilChange` · `GeneralRevision` · `Testing` · `Painting` · `Landscaping` · `Other`. |
| **Descripción (`description`)** | `string` max 2000 | Sí | Descripción detallada de la actividad a realizar. |
| **Frecuencia en Días (`frequencyDays`)** | `int` | Sí | Intervalo en días entre cada mantenimiento. Determina la fecha del próximo mantenimiento. |
| **Proveedor Preferido (`preferredProviderId`)** | `Guid?` FK | No | Proveedor preferido para ejecutar esta actividad. FK a `erp_providers`. |
| **Costo Estimado (`estimatedCost`)** | `decimal(18,2)` | No | Costo estimado por intervención para efectos presupuestales. |
| **Requiere Suspensión del Servicio (`requiresServiceSuspension`)** | `boolean` | Sí | Indica si el servicio debe suspenderse durante la ejecución. |
| **Horas Fuera de Servicio (`estimatedDowntimeHours`)** | `int` | No | Estimado de tiempo fuera de servicio en horas. |
| **Activo (`isActive`)** | `boolean` | Sí | `false` si el plan fue descontinuado. |
| **Última Ejecución (`lastExecutionDate`)** | `datetime` | No | Fecha de la última ejecución del plan. Se actualiza automáticamente al completar una orden preventiva. |
| **Próxima Ejecución (`nextExecutionDate`)** | `datetime` | No | Fecha programada del próximo mantenimiento. Se recalcula automáticamente: `fecha ejecución real + frequencyDays`. |

> [!IMPORTANT]
> **Cálculo de próxima fecha**: Al completar una orden de trabajo preventivo, el sistema suma `frequencyDays` a la **fecha de ejecución real** (no a la fecha programada) para evitar que los retrasos acumulen desfases en el calendario.

### 10.4 Orden de Trabajo (`WorkOrder`)

Registro de cada intervención de mantenimiento, ya sea preventivo (programado) o correctivo (atención de falla).

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Tipo de Orden (`orderType`)** | `Enum` | Sí | `Preventive` = Generada por plan de mantenimiento · `Corrective` = Atención de falla o daño. |
| **Bien (`assetId`)** | `Guid` FK | Sí | Bien afectado. Cascade delete. |
| **Descripción (`description`)** | `string` max 4000 | Sí | Trabajo a realizar o falla detectada. |
| **Prioridad (`priority`)** | `Enum` | Sí | `Emergency` = Emergencia (bien esencial dañado) · `High` = Alta · `Medium` = Media (default) · `Low` = Baja. |
| **Origen (`origin`)** | `Enum` | Sí | `AutomaticScheduling` = Generada por el motor preventivo · `AdminReport` = Reporte del administrador · `ResidentPqr` = Originada desde una PQR de residente. |
| **PQR Relacionada (`relatedPqrId`)** | `Guid?` FK | No | Referencia a la PQR que originó la orden (si aplica). |
| **Nro. PQR (`relatedPqrNumber`)** | `string` max 50 | No | Número de radicado de la PQR relacionada. |
| **Proveedor Asignado (`assignedProviderId`)** | `Guid?` FK | No | Proveedor encargado de ejecutar el trabajo. FK a `erp_providers`. |
| **Fecha Programada (`scheduledDate`)** | `datetime` | No | Fecha programada de ejecución. |
| **Fecha Inicio Ejecución (`executionStartDate`)** | `datetime` | No | Fecha real en que inició el trabajo. Se asigna automáticamente al cambiar estado a `InProgress`. |
| **Fecha Fin Ejecución (`executionEndDate`)** | `datetime` | No | Fecha real de finalización. Se asigna automáticamente al cambiar estado a `Completed`. |
| **Costo Estimado (`estimatedCost`)** | `decimal(18,2)` | No | Costo estimado de la intervención. |
| **Costo Real (`actualCost`)** | `decimal(18,2)` | No | Costo real de la intervención. Si supera el estimado en >20%, se genera alerta. |
| **Cuenta Presupuestal (`budgetAccountId`)** | `Guid?` FK | No | Cuenta del PUC a imputar el gasto. FK a `erp_accounting_accounts`. |
| **Asiento Contable (`accountingEntryId`)** | `Guid?` FK | No | Asiento contable generado por la imputación del costo. FK a `erp_accounting_entries`. |
| **Estado (`status`)** | `Enum` | Sí | `PendingAssignment` = Pendiente de asignación · `Assigned` = Asignada a proveedor · `InProgress` = En ejecución · `Completed` = Completada · `Cancelled` = Cancelada. |
| **Resultado (`outcome`)** | `Enum` | No | `Resolved` = Resuelto · `PartiallyResolved` = Resuelto parcialmente · `NotResolved` = No resuelto. |
| **Notas del Resultado (`outcomeNotes`)** | `string` max 2000 | No | Justificación del resultado de la intervención. |
| **Alerta de Costo Enviada (`costAlertSent`)** | `boolean` | Sí | `true` si se envió alerta por desviación >20% en costo. Evita envíos duplicados. |
| **Creado Por (`createdByUserId`)** | `string` max 450 | Automático | ID del usuario que creó la orden. |
| **Actualizado Por (`updatedByUserId`)** | `string` max 450 | No | ID del último usuario que modificó la orden. |

#### Transiciones de Estado

| Estado Origen | Estados Permitidos | Requisitos |
|---------------|-------------------|------------|
| `PendingAssignment` | `Assigned`, `Cancelled` | Para `Assigned`: debe tener proveedor asignado. |
| `Assigned` | `InProgress`, `Cancelled` | Se asigna fecha de inicio automáticamente. |
| `InProgress` | `Completed`, `Cancelled` | Se asigna fecha de fin automáticamente. |
| `Completed` | — | Estado final. Se recalcula `nextExecutionDate` del plan si es preventivo. Se actualiza PQR si originó desde PQR. |
| `Cancelled` | — | Estado final. |

### 10.5 Evidencia de Orden de Trabajo (`WorkOrderEvidence`)

Fotografías antes y después de cada intervención para evidenciar el trabajo realizado.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Orden de Trabajo (`workOrderId`)** | `Guid` FK | Sí | Orden a la que pertenece la evidencia. Cascade delete. |
| **Ruta del Archivo (`filePath`)** | `string` max 500 | Sí | Ruta de almacenamiento de la imagen. |
| **Descripción (`description`)** | `string` max 500 | No | Descripción de la evidencia. |
| **Es Antes de la Intervención (`isBeforeIntervention`)** | `boolean` | Sí | `true` = foto antes del trabajo · `false` = foto después del trabajo. |
| **Fecha de Captura (`capturedAt`)** | `datetime` | Automático | Fecha y hora de captura. |
| **Capturado Por (`capturedByUserId`)** | `string` max 450 | Automático | ID del usuario. |

### 10.6 Siniestro (`Incident`)

Registro de eventos extraordinarios (inundación, incendio, daño estructural) que agrupan múltiples órdenes de trabajo relacionadas.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Nombre (`name`)** | `string` max 300 | Sí | Nombre descriptivo del siniestro. Ej. "Inundación Torre A Nivel 1". |
| **Descripción (`description`)** | `string` max 4000 | No | Detalle del evento. |
| **Tipo de Siniestro (`incidentType`)** | `Enum` | Sí | `Flood` = Inundación · `Fire` = Incendio · `StructuralDamage` = Daño Estructural · `ElectricalFailure` = Falla Eléctrica · `Other` = Otro. |
| **Fecha de Ocurrencia (`occurredAt`)** | `datetime` | Sí | Fecha y hora en que ocurrió el siniestro. |
| **Valor Total del Daño (`totalDamageValue`)** | `decimal(18,2)` | No | Valor total estimado del daño en COP. |
| **Nro. Póliza de Seguro (`insurancePolicyNumber`)** | `string` max 100 | No | Número de la póliza de seguro del contrato de seguros registrado en el módulo de Proveedores. |
| **Aseguradora (`insuranceCompany`)** | `string` max 200 | No | Nombre de la empresa aseguradora. |
| **Archivo de Póliza (`policyFilePath`)** | `string` max 500 | No | Ruta del documento de la póliza digitalizada. |
| **Estado (`status`)** | `string` max 30 | Sí | `"Open"` = Abierto · `"Closed"` = Cerrado. |
| **Creado Por (`createdByUserId`)** | `string` max 450 | Automático | ID del usuario que registró el siniestro. |

### 10.7 Siniestro-Orden de Trabajo (`IncidentWorkOrder`)

Tabla asociativa que vincula un siniestro con las órdenes de trabajo relacionadas.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Siniestro (`incidentId`)** | `Guid` FK | Sí | Siniestro al que pertenece la relación. Cascade delete. |
| **Orden de Trabajo (`workOrderId`)** | `Guid` FK | Sí | Orden de trabajo vinculada. Cascade delete. |

> [!NOTE]
> Índice único por `(TenantId, IncidentId)` para garantizar que una orden solo pertenezca a un siniestro.

### 10.8 Historial de Estado del Bien (`AssetStatusHistory`)

Registro de cada cambio de estado de un bien con fecha, motivo y usuario responsable.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Bien (`assetId`)** | `Guid` FK | Sí | Bien que cambió de estado. Cascade delete. |
| **Estado Anterior (`previousStatus`)** | `Enum` | Sí | Estado antes del cambio. |
| **Estado Nuevo (`newStatus`)** | `Enum` | Sí | Estado después del cambio. |
| **Motivo (`reason`)** | `string` max 1000 | No | Justificación del cambio de estado. |
| **Cambiado Por (`changedByUserId`)** | `string` max 450 | Automático | ID del usuario que realizó el cambio. |
| **Nombre del Usuario (`changedByUserName`)** | `string` max 300 | Automático | Nombre visible del usuario. |
| **Fecha del Cambio (`changedAt`)** | `datetime` | Automático | Fecha y hora del cambio. |

### 10.9 Reglas de Negocio del Módulo

1. **Generación automática de órdenes preventivas**: El motor `PreventiveMaintenanceEngineService` ejecuta cada 6 horas y genera órdenes de trabajo preventivo con **7 días de anticipación** (configurable) según el plan de mantenimiento de cada bien. Las órdenes se crean en estado `PendingAssignment` o `Assigned` (si el plan tiene proveedor preferido).

2. **Alerta de orden sin asignar**: Si una orden de trabajo preventivo llega a su fecha de ejecución sin haber sido asignada a un proveedor, el sistema genera una **alerta crítica** visible en el dashboard.

3. **Recálculo de próxima fecha**: Al completar una orden de trabajo preventivo, el sistema calcula la fecha del próximo mantenimiento sumando la **frecuencia configurada** a la **fecha de ejecución real** (no a la fecha programada) para evitar que los retrasos acumulen desfases.

4. **Bloqueo de bienes fuera de servicio**: Un bien marcado como `OutOfService` debe aparecer en el tablero de alertas y **impedir** que esa zona o equipo sea reservada en el módulo de Reservas. Si el bien es esencial, la alerta se **escala al Consejo de Administración**.

5. **Imputación automática de costos**: El costo real de cada orden de trabajo debe imputarse a la cuenta presupuestal configurada, generando el asiento contable correspondiente sin intervención manual del contador.

6. **Alerta de desviación de costo**: Si el costo real supera el costo estimado en más del **20%**, el sistema alerta al administrador y al consejo antes de confirmar el registro del gasto.

7. **Actualización automática de PQR**: Una orden de trabajo originada desde una PQR de residente actualiza automáticamente el estado de esa PQR a `Responded` cuando la orden es completada, notificando al residente.

8. **Historial inmutable**: El historial completo de mantenimientos de cada bien se conserva indefinidamente como evidencia ante posibles reclamaciones de responsabilidad civil.

9. **Bienes dados de baja**: Los bienes en estado `Decommissioned` permanecen en el sistema con su historial pero marcados como inactivos. No pueden tener nuevos planes ni órdenes de trabajo.

10. **Reporte de mantenimientos programados**: El administrador puede generar reportes para los próximos 30, 60 o 90 días mostrando el costo estimado total por período y comparándolo contra el saldo disponible en la cuenta presupuestal correspondiente.

11. **Registro de siniestros**: Si el conjunto registra un siniestro (inundación, incendio, daño estructural), el administrador puede crear un evento que agrupe todas las órdenes de trabajo relacionadas, el valor total del daño y la referencia a la póliza de seguro del contrato registrado en el módulo de Proveedores.

---

## 11. Módulo de Comunicados y Notificaciones

Gestión de toda la comunicación oficial entre la administración y los residentes, tanto comunicados masivos formales como notificaciones automáticas generadas por eventos de otros módulos. Este módulo tiene implicaciones legales porque en Colombia la notificación debida es requisito de validez para muchos actos administrativos de la propiedad horizontal.

### 11.1 Comunicado (`Communication`)

Registro inmutable de cada comunicación formal enviada por la administración. Un comunicado puede ser inmediato, programado o guardado como borrador.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Asunto (`subject`)** | `string` max 500 | Sí | Título del comunicado. |
| **Cuerpo (`body`)** | `string` (longtext) | Sí | Contenido del comunicado con formato enriquecido. |
| **Estado (`status`)** | `Enum` | Sí | `Draft` = Borrador · `Scheduled` = Programado · `Sent` = Enviado · `Archived` = Archivado. Una vez enviado no puede modificarse. |
| **Tipo de Audiencia (`audienceType`)** | `Enum` | Sí | `AllOwners` = Todos los propietarios · `AllResidents` = Todos los residentes · `SpecificUnits` = Unidades específicas · `SpecificTowers` = Torres específicas · `CustomGroup` = Grupo personalizado. |
| **Canales Seleccionados (`selectedChannels`)** | `string` (JSON) | Sí | Lista separada por comas de los canales habilitados: `Email`, `Sms`, `Push`, `BulletinBoard`. |
| **Fecha de Envío (`sendAt`)** | `datetime` | No | Fecha y hora programada para envío futuro. `null` = envío inmediato o borrador. |
| **Fecha de Envío Real (`sentAt`)** | `datetime` | No | Se asigna automáticamente cuando el comunicado se envía. |
| **Requiere Confirmación de Lectura (`requiresReadConfirmation`)** | `boolean` | Sí | `true` = el sistema exige que el destinatario confirme haber leído el comunicado. |
| **Publicar en Cartelera (`publishToBulletinBoard`)** | `boolean` | Sí | `true` = se publica automáticamente en la cartelera digital. |
| **Comunicado Relacionado (`relatedCommunicationId`)** | `Guid?` FK | No | Referencia al comunicado anterior que este corrige o reemplaza. Self-reference. |
| **Archivos Adjuntos (`filePaths`)** | `string` (JSON) | No | Lista serializada de rutas de archivos adjuntos. |
| **Creado Por (`createdByUserId`)** | `string` max 450 | Automático | ID del usuario que creó el comunicado. |
| **Actualizado Por (`updatedByUserId`)** | `string` max 450 | No | ID del último usuario que modificó el comunicado. |

> [!IMPORTANT]
> **Inmutabilidad**: Una vez enviado, un comunicado no puede modificarse ni eliminarse. Solo se puede archivar (soft delete). Si se requiere corregir, debe crear un nuevo comunicado con referencia al anterior.

### 11.2 Destinatario del Comunicado (`CommunicationRecipient`)

Registro de entrega por cada destinatario de un comunicado. Almacena el estado de entrega individual por canal.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Comunicado (`communicationId`)** | `Guid` FK | Sí | Comunicado al que pertenece. Cascade delete. |
| **Propietario (`ownerId`)** | `Guid?` FK | No | ID del propietario destinatario (si el destinatario es propietario). FK a `erp_owners`. |
| **Arrendatario (`tenantResidentId`)** | `Guid?` FK | No | ID del arrendatario destinatario (si aplica). FK a `erp_tenant_residents`. |
| **Email del Destinatario (`recipientEmail`)** | `string` max 300 | No | Email del destinatario al momento del envío (snapshot). |
| **Teléfono del Destinatario (`recipientPhone`)** | `string` max 50 | No | Teléfono del destinatario al momento del envío (snapshot). |
| **Estado Email (`emailStatus`)** | `Enum` | Sí | `Pending` · `Sent` · `Delivered` · `Read` · `Failed` · `Bounced`. |
| **Estado SMS (`smsStatus`)** | `Enum` | Sí | `Pending` · `Sent` · `Delivered` · `Failed`. |
| **Estado Push (`pushStatus`)** | `Enum` | Sí | `Pending` · `Sent` · `Delivered`. |
| **Estado Cartelera (`bulletinBoardStatus`)** | `Enum` | Sí | `Pending` · `Sent` (se marca como enviado al publicarse en cartelera). |
| **Email Enviado (`emailSentAt`)** | `datetime` | No | Marca temporal del envío por correo. |
| **SMS Enviado (`smsSentAt`)** | `datetime` | No | Marca temporal del envío por SMS. |
| **Push Enviado (`pushSentAt`)** | `datetime` | No | Marca temporal del envío por notificación push. |
| **Confirmación de Lectura (`readConfirmedAt`)** | `datetime` | No | Fecha en que el destinatario confirmó la lectura del comunicado. |
| **Contador de Reenvíos (`resentCount`)** | `int` | Sí | Número de veces que se ha reenviado a este destinatario por no confirmar lectura. |
| **Último Reenvío (`lastResentAt`)** | `datetime` | No | Fecha del último reenvío. |
| **Mensaje de Error (`errorMessage`)** | `string` max 1000 | No | Descripción del error si el envío falló. |

### 11.3 Plantilla de Notificación (`NotificationTemplate`)

Plantillas configurables para las notificaciones automáticas, con variables dinámicas y versiones por canal.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Nombre (`name`)** | `string` max 200 | Sí | Nombre descriptivo de la plantilla. |
| **Tipo de Evento (`eventType`)** | `Enum` | Sí | `PaymentConfirmed` · `NewMonthlyBillingAvailable` · `DelinquencyNotice1` · `DelinquencyNotice2` · `DelinquencyNotice3` · `PreLegalNotice` · `PaymentAgreementConfirmed` · `PaymentAgreementDueSoon` · `PeaceAndSafetyIssued` · `PQRReceived` · `PQRStatusUpdated` · `PQRResponseAvailable` · `PQRClosed` · `ReservationApproved` · `ReservationRejected` · `ReservationReminder24h` · `ReservationReminder2h` · `DepositReturned` · `AssemblyConvocation` · `AssemblyReminder72h` · `AssemblyMinutesPublished` · `MaintenanceScheduled` · `OutOfService` · `WorkOrderResolved`. |
| **Para (`forRecipientType`)** | `Enum` | Sí | `Owner` = Propietario · `Tenant` = Arrendatario · `Both` = Ambos. |
| **Asunto Email (`emailSubject`)** | `string` max 500 | Sí | Asunto del correo electrónico. |
| **Cuerpo Email (`emailBody`)** | `string` (longtext) | Sí | Contenido del correo con formato enriquecido. |
| **Cuerpo SMS (`smsBody`)** | `string` max 160 | Sí | Texto plano del SMS. Máximo 160 caracteres. |
| **Variables Dinámicas (`dynamicVariables`)** | `string` (JSON) | No | Lista de nombres de variables que se reemplazan en el texto. Ej. `["Propietario","Unidad","Valor","Fecha"]`. |
| **Activo (`isActive`)** | `boolean` | Sí | `false` si la plantilla está desactivada y no debe usarse. |
| **Creado Por (`createdByUserId`)** | `string` max 450 | Automático | ID del usuario que creó la plantilla. |

### 11.4 Notificación Automática (`AutomaticNotification`)

Registro de cada notificación generada automáticamente por un evento de otro módulo.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Tipo de Evento (`eventType`)** | `Enum` | Sí | Tipo de evento que originó la notificación. Mismos valores que `NotificationTemplate.eventType`. |
| **Comunicado Relacionado (`communicationId`)** | `Guid?` FK | No | Si la notificación generó un comunicado formal, referencia al mismo. |
| **Propietario (`ownerId`)** | `Guid?` FK | No | Propietario destinatario de la notificación. |
| **Arrendatario (`tenantResidentId`)** | `Guid?` FK | No | Arrendatario destinatario de la notificación. |
| **Email Destino (`recipientEmail`)** | `string` max 300 | No | Email del destinatario (snapshot). |
| **Teléfono Destino (`recipientPhone`)** | `string` max 50 | No | Teléfono del destinatario (snapshot). |
| **Canal (`channel`)** | `Enum` | Sí | Canal por el que se envió: `Email` · `Sms` · `Push` · `BulletinBoard`. |
| **Estado (`status`)** | `Enum` | Sí | `Pending` · `Sent` · `Delivered` · `Read` · `Failed`. |
| **Enviado (`sentAt`)** | `datetime` | No | Fecha y hora del envío. |
| **Leído (`readAt`)** | `datetime` | No | Fecha en que el destinatario abrió/leyó la notificación. |
| **Módulo Origen (`sourceModule`)** | `string` max 50 | Sí | Módulo que originó el evento. `Billing` · `PQR` · `Reservations` · `Assembly` · `Maintenance`. |
| **ID Entidad Origen (`sourceEntityId`)** | `string` max 100 | Sí | ID de la entidad que generó el evento (UUID como string). |
| **Tipo Entidad Origen (`sourceEntityType`)** | `string` max 100 | Sí | Tipo de entidad origen. Ej. `"Payment"`, `"PqrRecord"`, `"Reservation"`. |
| **Mensaje de Error (`errorMessage`)** | `string` max 1000 | No | Mensaje si el envío falló. |

### 11.5 Preferencias de Comunicación (`CommunicationPreference`)

Configuración individual de canales de comunicación para cada residente. El sistema respeta estas preferencias para notificaciones no críticas.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Propietario (`ownerId`)** | `Guid?` FK | No | Propietario al que pertenecen las preferencias. FK a `erp_owners`. |
| **Arrendatario (`tenantResidentId`)** | `Guid?` FK | No | Arrendatario al que pertenecen las preferencias. FK a `erp_tenant_residents`. |
| **Permite Email (`allowEmail`)** | `boolean` | Sí | `true` = autoriza notificaciones por correo electrónico. |
| **Permite SMS (`allowSms`)** | `boolean` | Sí | `true` = autoriza notificaciones por SMS. |
| **Permite Push (`allowPush`)** | `boolean` | Sí | `true` = autoriza notificaciones push. |
| **Override Notificaciones Críticas (`criticalNotificationsOverride`)** | `boolean` | Sí | `true` = recibe notificaciones críticas (emergencia, cortes) sin importar las preferencias individuales. Default: `true`. |
| **Tipos Desuscritos (`unsubscribedEventTypes`)** | `string` (JSON) | No | Lista de tipos de evento de los que el residente solicitó no ser notificado. |
| **Notas (`notes`)** | `string` max 2000 | No | Notas internas del administrador sobre solicitudes de desuscripción. |
| **Cambiado Por (`changedByUserId`)** | `string` max 450 | Automático | ID del usuario que modificó las preferencias. |

> [!NOTE]
> Índice único por `(TenantId, OwnerId)` y `(TenantId, TenantResidentId)` con filtro `IS NOT NULL` para garantizar una sola preferencia por residente.

### 11.6 Publicación en Cartelera (`BulletinBoardPost`)

Publicaciones visibles en la cartelera digital del portal. Las publicaciones vencidas se archivan automáticamente.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Título (`title`)** | `string` max 300 | Sí | Título de la publicación. |
| **Contenido (`content`)** | `string` (longtext) | Sí | Contenido con formato enriquecido. |
| **Fecha de Publicación (`publishedAt`)** | `datetime` | Sí | Fecha desde la cual la publicación es visible. Default: fecha de creación. |
| **Fecha de Vencimiento (`expiresAt`)** | `datetime` | No | Fecha después de la cual la publicación se archiva automáticamente. |
| **Fijada al Tope (`isPinned`)** | `boolean` | Sí | `true` = aparece siempre al inicio de la cartelera, antes que las no fijadas. |
| **Categoría (`category`)** | `Enum` | Sí | `Administrative` = Administrativo · `Financial` = Financiero · `LivingTogether` = Convivencia · `Events` = Eventos · `Urgent` = Urgente. |
| **Creado Por (`createdByUserId`)** | `string` max 450 | Automático | ID del usuario que creó la publicación. |
| **Actualizado Por (`updatedByUserId`)** | `string` max 450 | No | ID del último usuario que modificó la publicación. |

> [!NOTE]
> Las publicaciones con `isPinned = true` se ordenan primero. Las vencidas se archivan automáticamente mediante el servicio programado, pero no se eliminan.

### 11.7 Configuración de Secuencia de Mora (`DelinquencySequenceConfig`)

Define los días y plantillas para cada aviso en la secuencia de cobro de mora.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Número de Paso (`stepNumber`)** | `int` | Sí | 1 = Primer aviso · 2 = Segundo aviso · 3 = Tercer aviso · 4 = Aviso prejurídico. |
| **Días después de Vencimiento (`daysAfterDue`)** | `int` | Sí | Número de días de mora para activar este paso. Ej. paso 1 = 1 día, paso 2 = 5 días, paso 3 = 15 días, paso 4 = 30 días. |
| **Plantilla (`templateId`)** | `Guid` FK | Sí | Plantilla de notificación a usar para este paso. Debe corresponder al tipo de evento `DelinquencyNotice1..3` o `PreLegalNotice`. FK a `erp_notification_templates`. |
| **Activo (`isActive`)** | `boolean` | Sí | `false` si el paso está desactivado y no debe procesarse. |

> [!IMPORTANT]
> Índice único por `(TenantId, StepNumber)`. La secuencia es progresiva: no se salta pasos.

### 11.8 Pausa de Secuencia de Mora (`DelinquencySequencePause`)

Suspende temporalmente la generación de avisos de mora para una unidad específica.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Unidad (`unitId`)** | `Guid` FK | Sí | Unidad a la que se aplica la pausa. FK a `erp_units`. |
| **Fecha Inicio (`startDate`)** | `datetime` | Sí | Fecha desde la cual la pausa está activa. |
| **Fecha Fin (`endDate`)** | `datetime` | No | Fecha hasta la cual la pausa está activa. `null` = indefinida (hasta que se elimine manualmente). |
| **Motivo (`reason`)** | `string` max 1000 | Sí | Razón de la pausa. Ej. "Acuerdo de pago vigente", "Reestructuración de deuda". |
| **Creado Por (`createdByUserId`)** | `string` max 450 | Automático | ID del usuario que registró la pausa. |

### 11.9 Reglas de Negocio del Módulo

1. **Inmutabilidad de comunicados enviados**: Todo comunicado enviado queda registrado con su contenido completo, lista de destinatarios, canales utilizados y estado de entrega por destinatario. No puede eliminarse ni modificarse. Solo puede archivarse (soft delete).

2. **Rastreo por canal**: El estado de entrega se rastrea individualmente por canal: para email se registra si fue entregado, abierto o rebotado; para SMS si fue enviado y entregado; para push si fue recibido.

3. **Reenvío a no confirmados**: Cuando un comunicado requiere confirmación de lectura, el administrador puede reenviarlo únicamente a quienes no han confirmado. El sistema incrementa `resentCount` y actualiza `lastResentAt`.

4. **Secuencia de mora progresiva**: Las notificaciones de mora siguen una secuencia configurable: primer aviso al día siguiente del vencimiento, segundo a los 5 días, tercero a los 15 y aviso prejurídico al día 30. Cada paso usa una plantilla distinta con tono progresivamente formal.

5. **Pausa por acuerdo de pago**: El administrador puede pausar la secuencia de avisos de mora para una unidad específica cuando existe un acuerdo de pago vigente. La pausa no afecta a las demás unidades.

6. **Notificaciones críticas**: Las notificaciones de emergencia o cortes de servicios se envían por todos los canales disponibles sin importar las preferencias individuales del residente.

7. **Notificaciones desde otros módulos**: Las convocatorias de asamblea se envían automáticamente usando todos los canales disponibles y quedan registradas como comunicados formales con confirmación de lectura obligatoria.

8. **Edición de comunicados programados**: Un comunicado en estado `Scheduled` puede editarse o cancelarse antes de su hora de envío. Una vez enviado no puede modificarse. Para corregir un comunicado enviado debe crearse uno nuevo con referencia al anterior.

9. **Preferencias de comunicación**: El sistema respeta las preferencias de canal configuradas por cada residente para notificaciones no críticas. Si un residente ha solicitado no recibir cierto tipo de comunicaciones, el administrador registra esa preferencia con constancia de la solicitud.

10. **Archivo automático de cartelera**: Las publicaciones vencidas se archivan automáticamente sin eliminarse. La cartelera muestra las vigentes con las fijadas al tope siempre primero.

---

## 12. Estándar de Campos en Frontend

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

---

## 13. Módulo de Reportes y Exportaciones

Gestión integral de reportes predefinidos, exportaciones generadas, configuración de reportes recurrentes, secciones del generador de informes anuales y plantillas de personalización PDF para la copropiedad.

### 13.1 Tabla: `erp_report_types`

Catálogo de tipos de reporte disponibles en el sistema. Se siembran 26 reportes estándar en la primera migración.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Id** | `Guid` | PK | Identificador único del tipo de reporte. |
| **TenantId** | `string` max 255 | FK | Identificador del conjunto. FK a `erp_tenant_configuration`. |
| **ReportTypeCode** | `string` max 40 | Sí | Código único del tipo de reporte. Enum: `ReportTypeEnum`. |
| **Name** | `string` max 200 | Sí | Nombre legible del reporte. |
| **Description** | `string` max 1000 | No | Descripción detallada del contenido y propósito del reporte. |
| **Category** | `string` max 20 | Sí | Categoría del reporte. Enum: `ReportCategory` — `Financial` · `Portfolio` · `Operational` · `Assembly` · `Annual`. |
| **SourceModules** | `string` max 500 | No | Módulos del sistema que alimentan el reporte, separados por coma. |
| **AllowedRoles** | `string` max 500 | No | Roles autorizados para ver este reporte, separados por coma. Controla la visibilidad basada en roles. |
| **ContainsPersonalData** | `boolean` | Sí | `true` si el reporte incluye datos personales de propietarios/residentes. Activa nota de confidencialidad en el pie de página del PDF. |
| **IsActive** | `boolean` | Sí | `false` si el tipo de reporte fue desactivado y no debe ofrecerse en la UI. |

> [!IMPORTANT]
> **Índices**: Único `(TenantId, ReportTypeCode)` · Compuesto `(TenantId, Category)`.
>
> **Reportes estándar**: Se siembran 26 tipos de reporte en la primera migración del sistema cubriendo las categorías Financial, Portfolio, Operational, Assembly y Annual. No pueden eliminarse, solo desactivarse.

### 13.2 Tabla: `erp_generated_reports`

Registro de cada reporte generado y exportado por los usuarios, con su formato, archivo resultante y metadatos de generación.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Id** | `Guid` | PK | Identificador único del reporte generado. |
| **TenantId** | `string` max 255 | Sí | Identificador del conjunto. |
| **ReportTypeId** | `Guid` | FK | Tipo de reporte generado. FK a `erp_report_types`. |
| **Format** | `string` max 10 | Sí | Formato de exportación. Enum: `ReportFormat` — `Pdf` · `Excel` · `Csv`. |
| **PeriodFrom** | `DateTime?` | No | Fecha inicial del período filtrado en el reporte. |
| **PeriodTo** | `DateTime?` | No | Fecha final del período filtrado en el reporte. |
| **FileName** | `string` max 500 | Sí | Nombre del archivo generado (sin ruta). |
| **FilePath** | `string` max 1000 | Sí | Ruta física completa del archivo almacenado. |
| **FileSizeBytes** | `long` | Sí | Tamaño del archivo en bytes. |
| **GeneratedByUserId** | `string` max 450 | Sí | ID del usuario que generó el reporte. FK a `AspNetUsers`. |
| **GeneratedAt** | `DateTime` | Automático | Fecha y hora de generación del reporte. |
| **Parameters** | `text` | No | Parámetros usados para la generación serializados en JSON. |
| **Notes** | `text` | No | Notas u observaciones sobre el reporte generado. |
| **RecurringConfigId** | `Guid?` | FK | Configuración recurrente que originó esta generación (si aplica). FK a `erp_recurring_report_configs`. |

> [!IMPORTANT]
> **Índices**: Compuesto `(TenantId, ReportTypeId)`.
>
> **Almacenamiento**: Los archivos se guardan en `wwwroot/reports/{tenantId}/{reportTypeCode}/{fileName}`. `FileSizeBytes` almacena el tamaño en bytes. `GeneratedByUserId` referencia a la tabla `AspNetUsers` del identity.

### 13.3 Tabla: `erp_recurring_report_configs`

Configuración de reportes programados para generación automática y recurrente según una frecuencia definida.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Id** | `Guid` | PK | Identificador único de la configuración recurrente. |
| **TenantId** | `string` max 255 | Sí | Identificador del conjunto. |
| **ReportTypeId** | `Guid` | FK | Tipo de reporte a generar recurrentemente. FK a `erp_report_types`. |
| **Name** | `string` max 200 | Sí | Nombre descriptivo de la configuración. Ej. "Cartera mensual a tesorería". |
| **Frequency** | `string` max 15 | Sí | Frecuencia de generación. Enum: `ReportFrequency` — `Daily` · `Weekly` · `Monthly` · `Quarterly` · `Annual`. |
| **Format** | `string` max 10 | Sí | Formato de exportación. Enum: `ReportFormat` — `Pdf` · `Excel` · `Csv`. |
| **RecipientEmails** | `text` | No | Lista de correos electrónicos destinatarios, separados por coma o punto y coma. |
| **SubjectTemplate** | `string` max 500 | No | Plantilla del asunto del correo de envío. Puede incluir variables como `{ReportName}`, `{Period}`. |
| **BodyTemplate** | `text` | No | Plantilla del cuerpo del correo de envío. |
| **LastExecutionAt** | `DateTime?` | No | Fecha y hora de la última ejecución. |
| **NextExecutionAt** | `DateTime?` | No | Fecha y hora calculada para la próxima ejecución. Se recalcula automáticamente según la frecuencia. |
| **Status** | `string` max 10 | Sí | Estado de la configuración. Enum: `ReportRecurrentStatus` — `Active` · `Paused` · `Completed`. |
| **CreatedAt** | `DateTime` | Automático | Fecha de creación de la configuración. |
| **CreatedByUserId** | `string` max 450 | Sí | ID del usuario que creó la configuración. |

> [!IMPORTANT]
> **API vs Base de datos**: El campo `RecipientEmails` se almacena en base de datos como cadena separada por comas (`string.Join(",")`), pero en los DTOs de la API se expone como `List<string>` para facilitar su consumo desde el frontend. El controlador realiza la conversión en la creación (`string.Join`) y en la respuesta (`Split`).
>
> **Resolución de tipo de reporte**: El endpoint `POST /api/report/recurring` recibe `ReportTypeCode` (string) en lugar de `ReportTypeId` (Guid) para coincidir con el select del frontend que usa `r.code` como valor de opción. El controlador busca el `ReportType` por código + tenant antes de crear la configuración.
>
> El motor `RecurringReportEngine` ejecuta cada 5 minutos consultando configuraciones activas con `NextExecutionAt ≤ now`. Al ejecutarse, recalcula `NextExecutionAt` sumando la frecuencia a la fecha actual.

### 13.4 Tabla: `erp_management_report_sections`

Secciones del generador de informes incrementales para el reporte anual de gestión. Permite construir el informe de forma colaborativa combinando secciones autogeneradas con edición manual.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Id** | `Guid` | PK | Identificador único de la sección. |
| **TenantId** | `string` max 255 | Sí | Identificador del conjunto. |
| **ReportTypeCode** | `string` max 40 | Sí | Código del tipo de reporte al que pertenece la sección. |
| **SectionOrder** | `int` | Sí | Orden de aparición de la sección en el informe. |
| **Title** | `string` max 200 | Sí | Título de la sección. |
| **Content** | `text` | Sí | Contenido de la sección en formato enriquecido. |
| **Status** | `string` max 20 | Sí | Estado de la sección. Enum: `SectionStatus` — `Pending` · `AutoGenerated` · `ManuallyEdited`. |
| **AutoGeneratedQuery** | `text` | No | Consulta o configuración utilizada para la autogeneración del contenido. |
| **LastAutoGeneratedAt** | `DateTime?` | No | Fecha de la última autogeneración. |
| **LastManualEditAt** | `DateTime?` | No | Fecha de la última edición manual. |
| **LastEditedByUserId** | `string` max 450 | No | ID del usuario que realizó la última edición manual. |
| **CreatedAt** | `DateTime` | Automático | Fecha de creación de la sección. |
| **UpdatedAt** | `DateTime?` | No | Fecha de la última actualización. |

> [!NOTE]
> Esta tabla soporta el **Incremental Annual Report Builder**. Las secciones pueden regenerarse automáticamente mediante consultas predefinidas (`AutoGeneratedQuery`) o ser editadas manualmente para el informe anual de gestión.

### 13.5 Tabla: `erp_pdf_templates`

Personalización visual de los reportes PDF generados por el sistema. Cada conjunto puede definir su propia plantilla con logo, colores corporativos, firmas y notas legales.

| Campo | Tipo de Dato | Obligatorio | Descripción / Reglas |
|-------|--------------|-------------|----------------------|
| **Id** | `Guid` | PK | Identificador único de la plantilla. |
| **TenantId** | `string` max 255 | Sí | Identificador del conjunto. |
| **ReportTypeCode** | `string` max 40 | Sí | Código del tipo de reporte al que aplica esta plantilla. |
| **LogoFilePath** | `string` max 500 | No | Ruta del archivo de imagen del logo del conjunto. |
| **HeaderText** | `string` max 500 | Sí | Texto del encabezado visible en cada página del PDF. |
| **FooterText** | `string` max 500 | Sí | Texto del pie de página. |
| **SignatureName** | `string` max 200 | Sí | Nombre de la persona que firma el reporte (ej. representante legal). |
| **SignatureRole** | `string` max 200 | Sí | Cargo de la persona que firma. Ej. "Representante Legal". |
| **ConfidentialityNote** | `text` | No | Nota de confidencialidad mostrada en reportes con datos personales (`ContainsPersonalData = true`). |
| **DisclaimerNote** | `text` | No | Nota de descargo mostrada en reportes financieros. |
| **PrimaryColor** | `string` max 7 | Sí | Color primario corporativo en formato hex. Default: `#059669` (verde esmeralda) para líneas de acento. |
| **SecondaryColor** | `string` max 7 | Sí | Color secundario corporativo en formato hex. Default: `#1e293b` (azul oscuro). |
| **IsDefault** | `boolean` | Sí | `true` si esta plantilla es la predeterminada para el tipo de reporte. |
| **CreatedAt** | `DateTime` | Automático | Fecha de creación de la plantilla. |
| **UpdatedAt** | `DateTime?` | No | Fecha de la última modificación. |
| **CreatedByUserId** | `string` max 450 | Sí | ID del usuario que creó la plantilla. |

> [!IMPORTANT]
> **Índices**: Compuesto `(TenantId, ReportTypeCode)`.
>
> **Colores**: `PrimaryColor` = verde esmeralda (`#059669`) usado para líneas de acento y bordes en el PDF. `SecondaryColor` = azul oscuro (`#1e293b`) usado para textos secundarios.
>
> **Notas condicionales**: `ConfidentialityNote` se muestra en el footer cuando el reporte contiene datos personales (`ContainsPersonalData = true`). `DisclaimerNote` se muestra en reportes de categoría financiera.

### 13.6 Reglas de Negocio Transversales

1. **Control de acceso basado en roles**: `AllowedRoles` determina qué roles pueden acceder a cada reporte. SuperAdmin y Admin ven los 26 reportes completos. Council excluye `OwnerRegistry` y reportes con datos personales. Accountant y Auditor ven reportes financieros, de cartera y `OwnerRegistry`. Resident solo accede a `PortfolioByUnit` de su propia unidad. En los endpoints del `ReportController`, los roles se validan como `role != "Admin" && role != "SuperAdmin"` (no solo `role != "Admin"`) porque el claim del token JWT usa `"SuperAdmin"` como valor.

2. **Generación PDF**: Se utiliza QuestPDF. El header incluye logo del conjunto (si configurado), nombre del conjunto, NIT y dirección. El footer contiene nombre y cargo del firmante, fecha de generación, notas de confidencialidad (si aplica) y disclaimer (si aplica).

3. **Generación Excel**: Se utiliza ClosedXML con formato profesional: números en notación `#,##0.00`, auto-filtro en la primera fila, colores alternos en filas y fila de totales al final.

4. **Almacenamiento de archivos**: Los reportes generados se almacenan en `wwwroot/reports/{tenantId}/{reportTypeCode}/{fileName}`.

5. **Motor recurrente**: El `RecurringReportEngine` ejecuta cada 5 minutos consultando todas las configuraciones activas con `NextExecutionAt ≤ now`. Genera el reporte, lo almacena y lo envía por correo a los destinatarios configurados. Luego recalcula `NextExecutionAt` según la frecuencia definida.

---

## 14. M�dulo de Asambleas

Gest�n del ciclo de vida completo de las asambleas de copropietarios (Ordinarias y Extraordinarias), incluyendo convocatoria, quorum, registro de asistencia, votaci�n por puntos del orden del d�a, generaci�n de actas y propagaci�n de decisiones a otros m�dulos del sistema.

> [!IMPORTANT]
> Este m�dulo implementa los requisitos de la **Ley 675 de 2001** para propiedades horizontales en Colombia: convocatoria con antelaci�n m�nima, c�lculo de quorum (primera y segunda convocatoria), mayor�as calificadas y simples, y registro de constancias.

### 14.1 Asamblea (`Assembly`) � Tabla: `erp_assemblies`

Cabecera de cada asamblea. Contiene los datos de planificaci�n, fechas, umbrales de quorum y estado del ciclo de vida.

| Campo | Tipo de Dato | Obligatorio | Descripci�n / Reglas |
|-------|--------------|-------------|----------------------|
| **Tipo (`type`)** | `Enum` string | S� | `Ordinary` = Ordinaria (anual, obligatoria por ley) � `Extraordinary` = Extraordinaria (convocada para temas espec�ficos). |
| **Estado (`status`)** | `Enum` string | S� | `Draft` = Planificaci�n � `Convoked` = Convocada � `InSession` = En sesi�n � `Closed` = Cerrada � `MinutesApproved` = Acta aprobada � `Published` = Acta publicada. |
| **Tipo de Participaci�n (`participationType`)** | `Enum` string | S� | `InPerson` = Presencial � `Remote` = Virtual � `Hybrid` = Mixta. |
| **T�tulo (`title`)** | `string` max 500 | S� | T�tulo descriptivo. Ej. "Asamblea General Ordinaria 2026". |
| **Descripci�n (`description`)** | `string` max 4000 | No | Informaci�n adicional sobre el prop�sito de la asamblea. |
| **Fecha Programada (`scheduledDate`)** | `datetime` | S� | Fecha de la asamblea en primera convocatoria. |
| **Hora Programada (`scheduledTime`)** | `string` max 10 | S� | Hora de inicio en formato HH:mm. |
| **Lugar (`location`)** | `string` max 300 | S� | Direcci�n o sala donde se realizar� la asamblea. |
| **Fecha 2da Convocatoria (`secondConvocationDate`)** | `datetime` | No | Fecha alternativa si no se alcanza quorum en primera convocatoria. |
| **Hora 2da Convocatoria (`secondConvocationTime`)** | `string` max 10 | No | Hora de la segunda convocatoria. |
| **Lugar 2da Convocatoria (`secondConvocationLocation`)** | `string` max 300 | No | Lugar de la segunda convocatoria (puede ser el mismo). |
| **Coeficiente Total (`totalCoefficients`)** | `decimal(18,4)` | Autom�tico | Suma de coeficientes de todas las unidades activas. Se calcula al momento de convocar. |
| **Umbral Quorum 1ra (`quorumThresholdFirstCall`)** | `decimal(18,4)` | Autom�tico | `totalCoefficients � 0.50` (mitad m�s uno de coeficientes representados). |
| **Umbral Quorum 2da (`quorumThresholdSecondCall`)** | `decimal(18,4)` | Autom�tico | `totalCoefficients � 0.25` (m�nimo 25% del coeficiente total para segunda convocatoria). |
| **Quorum Alcanzado 1ra (`quorumAchievedFirstCall`)** | `boolean` | S� | Se marca `true` cuando la asistencia registrada supera el umbral de primera convocatoria. Si es `false`, se procede a segunda convocatoria. |
| **Quorum Alcanzado 2da (`quorumAchievedSecondCall`)** | `boolean` | S� | `true` si se alcanz� quorum en segunda convocatoria. |
| **N�mero de Convocatoria (`convocationNumber`)** | `int` | Autom�tico | `1` = Primera convocatoria � `2` = Segunda convocatoria (se incrementa autom�ticamente). |
| **Inicio de Sesi�n (`sessionStartTime`)** | `datetime` | No | Fecha/hora real de inicio registrada por el presidente. |
| **Fin de Sesi�n (`sessionEndTime`)** | `datetime` | No | Fecha/hora real de cierre. |
| **Presidente (`presidentName`)** | `string` max 300 | No | Nombre del propietario elegido como presidente de la asamblea. |
| **Secretario (`secretaryName`)** | `string` max 300 | No | Nombre del propietario elegido como secretario. |
| **ID Propietario Presidente (`presidentOwnerId`)** | `string` max 100 | No | ID del propietario que ejerce como presidente. |
| **ID Propietario Secretario (`secretaryOwnerId`)** | `string` max 100 | No | ID del propietario que ejerce como secretario. |
| **Convocatoria Enviada (`convocationSentAt`)** | `datetime` | No | Marca temporal del env�o masivo de la convocatoria. |
| **Plazo Convocatoria Cumplido (`convocationDeadlineMet`)** | `boolean` | S� | `true` si la convocatoria se envi� con al menos 10 d�as h�biles de antelaci�n (Ordinaria) o 7 d�as (Extraordinaria). |
| **Creado Por (`createdByUserId`)** | `string` max 450 | Autom�tico | ID del usuario que cre� la asamblea. |
| **Actualizado Por (`updatedByUserId`)** | `string` max 450 | No | ID del �ltimo usuario que modific� la asamblea. |

> [!NOTE]
> **�ndices**: `(TenantId, Status)`, `(TenantId, ScheduledDate)`, `(TenantId, Type)`. Soft delete aplicado. Todos los hijos (convocatorias, asistencias, agenda, etc.) se eliminan en cascada al eliminar la asamblea.

### 14.2 Convocatoria (`AssemblyConvocation`) � Tabla: `erp_assembly_convocations`

Registro de cada env�o de convocatoria asociado a una asamblea. Pueden existir m�ltiples convocatorias (primera, segunda, reenv�os).

| Campo | Tipo de Dato | Obligatorio | Descripci�n / Reglas |
|-------|--------------|-------------|----------------------|
| **Asamblea (`assemblyId`)** | `Guid` FK | S� | Asamblea a la que pertenece la convocatoria. Cascade delete. |
| **N�mero de Convocatoria (`convocationNumber`)** | `int` | S� | `1` = Primera � `2` = Segunda. |
| **Asunto (`subject`)** | `string` max 500 | S� | Asunto del mensaje de convocatoria. |
| **Notas (`notes`)** | `string` max 4000 | No | Instrucciones adicionales o notas para los destinatarios. |
| **Fecha de Env�o (`sentAt`)** | `datetime` | No | Fecha en que se proces� el env�o masivo. |
| **Enviado Por (`sentByUserId`)** | `string` max 450 | S� | ID del usuario que envi� la convocatoria. |
| **Canal (`channel`)** | `Enum` string | S� | `Email` � `Sms` � `PortalNotification`. Canal principal de env�o. |
| **Total Destinatarios (`totalRecipients`)** | `int` | S� | N�mero de destinatarios a los que se intent� enviar. |
| **Entregados (`deliveredCount`)** | `int` | S� | N�mero de env�os exitosos. |
| **Fallidos (`failedCount`)** | `int` | S� | N�mero de env�os que fallaron. |

> [!NOTE]
> **�ndices**: `(TenantId, AssemblyId)`. Al crear una convocatoria, el sistema genera autom�ticamente los registros en `erp_convocation_recipients` para todos los propietarios activos.

#### 14.2.1 Documento de Convocatoria (`ConvocationDocument`) � Tabla: `erp_convocation_documents`

Documentos adjuntos a la convocatoria (orden del d�a, estados financieros, soportes).

| Campo | Tipo de Dato | Obligatorio | Descripci�n / Reglas |
|-------|--------------|-------------|----------------------|
| **Convocatoria (`convocationId`)** | `Guid` FK | S� | Convocatoria a la que pertenece el documento. Cascade delete. |
| **Nombre (`documentName`)** | `string` max 300 | S� | Nombre descriptivo del documento. |
| **Tipo (`documentType`)** | `string` max 50 | S� | Tipo de documento. Ej. "PDF", "Excel", "Imagen". |
| **Ruta (`filePath`)** | `string` max 500 | S� | Ruta de almacenamiento del archivo. |
| **Descripci�n (`description`)** | `string` max 500 | No | Nota adicional sobre el contenido del documento. |

#### 14.2.2 Destinatario de Convocatoria (`ConvocationRecipient`) � Tabla: `erp_convocation_recipients`

Registro individual de entrega por cada propietario destinatario.

| Campo | Tipo de Dato | Obligatorio | Descripci�n / Reglas |
|-------|--------------|-------------|----------------------|
| **Convocatoria (`convocationId`)** | `Guid` FK | S� | Convocatoria asociada. Cascade delete. |
| **Unidad (`unitId`)** | `Guid` FK | S� | Unidad del destinatario. FK a `erp_units` (Restrict). |
| **Propietario (`ownerId`)** | `Guid` FK | S� | Propietario destinatario. FK a `erp_owners` (Restrict). |
| **Nombre (`ownerName`)** | `string` max 300 | S� | Nombre del propietario al momento del env�o (snapshot). |
| **Email (`ownerEmail`)** | `string` max 300 | S� | Email al momento del env�o (snapshot). |
| **Tel�fono (`ownerPhone`)** | `string` max 50 | No | Tel�fono al momento del env�o (snapshot). |
| **Entregado (`delivered`)** | `boolean` | S� | `true` si el env�o fue exitoso. |
| **Entregado El (`deliveredAt`)** | `datetime` | No | Fecha de entrega confirmada. |
| **Error (`deliveryError`)** | `string` max 500 | No | Descripci�n del error si fall� el env�o. |

> [!IMPORTANT]
> Los snapshots de nombre, email y tel�fono garantizan la trazabilidad legal: si el propietario cambia sus datos despu�s de la convocatoria, el registro hist�rico conserva los datos que ten�a al momento del env�o.

### 14.3 Registro de Asistencia (`AssemblyAttendance`) � Tabla: `erp_assembly_attendances`

Registro de cada propietario que asiste a la asamblea, con su representaci�n y derechos de voto.

| Campo | Tipo de Dato | Obligatorio | Descripci�n / Reglas |
|-------|--------------|-------------|----------------------|
| **Asamblea (`assemblyId`)** | `Guid` FK | S� | Asamblea a la que asiste. Cascade delete. |
| **Unidad (`unitId`)** | `Guid` FK | S� | Unidad representada. FK a `erp_units` (Restrict). |
| **Propietario (`ownerId`)** | `Guid` FK | S� | Propietario que asiste. FK a `erp_owners` (Restrict). |
| **Coeficiente (`coefficient`)** | `decimal(18,4)` | Autom�tico | Coeficiente de copropiedad de la unidad al momento de la asamblea. |
| **Estado (`status`)** | `Enum` string | S� | `Present` = Presente � `Represented` = Representado por poder � `Absent` = Ausente. |
| **Asiste Personalmente (`attendsPersonally`)** | `boolean` | S� | `true` = asiste en persona � `false` = representado por poder. |
| **Propietario Representante (`representativeOwnerId`)** | `Guid?` FK | Cond. | Si `attendsPersonally = false`, propietario que otorg� el poder. FK a `erp_owners` (SetNull). |
| **Nombre Representante (`representativeName`)** | `string` max 300 | Cond. | Nombre de la persona que ejerce la representaci�n. |
| **Documento Representante (`representativeDocumentNumber`)** | `string` max 50 | Cond. | Documento de identidad del representante. |
| **Ruta Poder (`powerOfAttorneyFilePath`)** | `string` max 500 | Cond. | Ruta del archivo del poder notarial escaneado. |
| **Hora Llegada (`arrivalTime`)** | `datetime` | S� | Hora de registro de asistencia. |
| **Hora Salida (`departureTime`)** | `datetime` | No | Hora de retiro (si aplica). |
| **Tiene Deuda (`hasDuesArrears`)** | `boolean` | S� | `true` si la unidad tiene cartera morosa al momento de la asamblea. |
| **Derecho de Voto Restringido (`votingRightRestricted`)** | `boolean` | S� | `true` si el propietario no puede votar por morosidad. |
| **Motivo Restricci�n (`votingRestrictionReason`)** | `string` max 1000 | Cond. | Explicaci�n si el derecho de voto est� restringido. |
| **Restricci�n Levantada Por (`votingRestrictionLiftedByUserId`)** | `string` max 450 | No | ID del usuario que levant� la restricci�n. |
| **Motivo Levantamiento (`votingRestrictionLiftedReason`)** | `string` max 1000 | Cond. | Raz�n del levantamiento de la restricci�n. |
| **Es Miembro Comisi�n (`isCommissionMember`)** | `boolean` | S� | `true` si el asistente es miembro de la comisi�n de revisi�n de acta. |
| **Rol Comisi�n (`commissionRole`)** | `string` max 100 | Cond. | Cargo en la comisi�n si aplica. |
| **Notas (`notes`)** | `string` max 2000 | No | Observaciones sobre la asistencia. |
| **Registrado Por (`registeredByUserId`)** | `string` max 450 | Autom�tico | ID del usuario que registr� la asistencia. |

> [!IMPORTANT]
> **�ndice �nico**: `(TenantId, AssemblyId, UnitId)` � una sola asistencia por unidad por asamblea. Los coeficientes se almacenan como snapshot al momento del registro para evitar que cambios posteriores en unidades alteren el hist�rico de la asamblea.


### 14.4 Punto del Orden del D�a (`AssemblyAgendaItem`) � Tabla: `erp_assembly_agenda_items`

Cada tema a tratar en la asamblea, con su sistema de votaci�n y resultado.

| Campo | Tipo de Dato | Obligatorio | Descripci�n / Reglas |
|-------|--------------|-------------|----------------------|
| **Asamblea (`assemblyId`)** | `Guid` FK | S� | Asamblea a la que pertenece. Cascade delete. |
| **N�mero de Orden (`sequenceNumber`)** | `int` | S� | Posici�n en el orden del d�a. |
| **T�tulo (`title`)** | `string` max 500 | S� | Nombre del punto a tratar. |
| **Descripci�n (`description`)** | `string` max 4000 | No | Detalle del tema. |
| **Presentador (`presenterName`)** | `string` max 300 | No | Nombre de la persona que presenta el punto. |
| **Mayor�a Requerida (`majorityRequired`)** | `Enum` string | S� | `Simple` = Mayor�a simple (mitad + 1) � `Qualified` = Mayor�a calificada (70%) � `Unanimity` = Unanimidad (100%). |
| **Modo de Votaci�n (`votingMode`)** | `Enum` string | S� | `Public` = Voto p�blico nominal � `Secret` = Voto secreto. |
| **Es Solo Informativo (`isInformationOnly`)** | `boolean` | S� | `true` = punto informativo, no requiere votaci�n. |
| **Requiere Votaci�n (`requiresVoting`)** | `boolean` | S� | `true` = se somete a votaci�n (default). |
| **Coeficientes Totales (`totalCoefficientsForVote`)** | `decimal(18,4)` | Autom�tico | Suma de coeficientes representados en la votaci�n. |
| **Coeficientes a Favor (`votesInFavorCoefficients`)** | `decimal(18,4)` | Autom�tico | Suma de coeficientes de votos a favor. |
| **Coeficientes en Contra (`votesAgainstCoefficients`)** | `decimal(18,4)` | Autom�tico | Suma de coeficientes de votos en contra. |
| **Coeficientes Abstenci�n (`abstentionCoefficients`)** | `decimal(18,4)` | Autom�tico | Suma de coeficientes de abstenciones. |
| **Votos a Favor (conteo) (`votesInFavorCount`)** | `int` | Autom�tico | N�mero de votos a favor (personas). |
| **Votos en Contra (conteo) (`votesAgainstCount`)** | `int` | Autom�tico | N�mero de votos en contra. |
| **Abstenciones (conteo) (`abstentionCount`)** | `int` | Autom�tico | N�mero de abstenciones. |
| **Aprobado (`isApproved`)** | `boolean?` | No | `true` = aprobado � `false` = rechazado � `null` = pendiente. |
| **Motivo Rechazo (`rejectionReason`)** | `string` max 1000 | Cond. | Explicaci�n si el punto fue rechazado. |
| **Observaciones (`observations`)** | `string` max 4000 | No | Notas del secretario sobre la discusi�n. |
| **Notas del Propietario (`ownerNotes`)** | `string` max 4000 | No | Notas visibles solo para el propietario que las registr�. |
| **Voto Registrado (`voteRegistered`)** | `boolean` | S� | `true` cuando se han registrado los resultados de la votaci�n. |
| **Registrado Por (`registeredByUserId`)** | `string` max 450 | No | ID del usuario que registr� la votaci�n. |
| **Voto Registrado El (`voteRegisteredAt`)** | `datetime` | No | Marca temporal del registro de votaci�n. |

> [!IMPORTANT]
> **Mayor�as seg�n Ley 675**: `Simple` = coeficientes a favor > coeficientes en contra + abstenciones (sobre los representados). `Qualified` = coeficientes a favor >= 70% de los coeficientes totales del conjunto. `Unanimity` = 100% de coeficientes totales.

### 14.5 Constancia (`AssemblyConstancy`) � Tabla: `erp_assembly_constancies`

Registro de constancias, objeciones o salvedades presentadas por un propietario durante la asamblea.

| Campo | Tipo de Dato | Obligatorio | Descripci�n / Reglas |
|-------|--------------|-------------|----------------------|
| **Asamblea (`assemblyId`)** | `Guid` FK | S� | Asamblea donde se presenta la constancia. Cascade delete. |
| **Punto del Orden del D�a (`agendaItemId`)** | `Guid?` FK | No | Punto al que se refiere la constancia. FK (SetNull). |
| **Propietario (`ownerId`)** | `Guid` FK | S� | Propietario que presenta la constancia. FK a `erp_owners` (Restrict). |
| **Nombre (`ownerName`)** | `string` max 300 | S� | Nombre del propietario (snapshot). |
| **Texto (`text`)** | `string` max 4000 | S� | Contenido de la constancia u objeci�n. |
| **Registrado Por (`registeredByUserId`)** | `string` max 450 | Autom�tico | ID del usuario que registr� la constancia. |

> [!NOTE]
> Las constancias forman parte integral del acta y no pueden eliminarse una vez registradas.


### 14.6 Acta (`AssemblyMinutes`) � Tabla: `erp_assembly_minutes`

Documento oficial de la asamblea. Pasa por estados: Draft -> UnderReview (comisi�n) -> Approved -> Published.

| Campo | Tipo de Dato | Obligatorio | Descripci�n / Reglas |
|-------|--------------|-------------|----------------------|
| **Asamblea (`assemblyId`)** | `Guid` FK | S� | Asamblea asociada. Cascade delete. |
| **Estado (`status`)** | `Enum` string | S� | `Draft` = Borrador � `UnderReview` = En revisi�n (comisi�n) � `Approved` = Aprobada � `Published` = Publicada a residentes. |
| **Presidente (`presidentName`)** | `string` max 300 | No | Nombre del presidente al momento de generar el acta. |
| **Secretario (`secretaryName`)** | `string` max 300 | No | Nombre del secretario al momento de generar el acta. |
| **Texto Completo (`fullText`)** | `string` (longtext) | S� | Contenido completo del acta en formato enriquecido. Generado autom�ticamente por `AssemblyMinutesGenerator`. |
| **Generada El (`generatedAt`)** | `datetime` | Autom�tico | Fecha de generaci�n del borrador del acta. |
| **Generada Por (`generatedByUserId`)** | `string` max 450 | Autom�tico | ID del usuario que gener� el acta. |
| **Miembros Comisi�n (`commissionMemberNames`)** | `string` max 2000 | No | Nombres de los miembros de la comisi�n revisora. |
| **Fecha L�mite Revisi�n (`commissionReviewDeadline`)** | `datetime` | No | Fecha m�xima para que la comisi�n emita comentarios. |
| **Comentarios Comisi�n (`commissionComments`)** | `string` max 4000 | No | Observaciones de la comisi�n revisora. |
| **Ruta Firma Presidente (`presidentSignatureFilePath`)** | `string` max 500 | No | Ruta de la imagen de la firma del presidente. |
| **Ruta Firma Secretario (`secretarySignatureFilePath`)** | `string` max 500 | No | Ruta de la imagen de la firma del secretario. |
| **Aprobada El (`approvedAt`)** | `datetime` | No | Fecha de aprobaci�n del acta. |
| **Aprobada Por (`approvedByUserId`)** | `string` max 450 | No | ID del usuario que aprob� el acta en el sistema. |
| **Publicada El (`publishedAt`)** | `datetime` | No | Fecha de publicaci�n del acta a los residentes. |
| **Publicada Por (`publishedByUserId`)** | `string` max 450 | No | ID del usuario que public� el acta. |
| **Conteo Notificaciones (`publishNotificationCount`)** | `int` | No | N�mero de notificaciones enviadas al publicar el acta. |
| **Notas de Revisi�n (`revisionNotes`)** | `string` max 4000 | No | Notas sobre correcciones solicitadas por la comisi�n. |

> [!IMPORTANT]
> **Ciclo del acta**: Al cerrar la asamblea, el sistema genera autom�ticamente un borrador de acta con los datos registrados (asistencia, resultados de votaci�n, constancias). La comisi�n designada revisa y aprueba. Una vez aprobada, se publica a los residentes y se env�a notificaci�n masiva. El acta publicada es **inmutable**.

### 14.7 Propagaci�n de Decisiones (`AssemblyDecisionPropagation`) � Tabla: `erp_assembly_decision_propagations`

Registro de las decisiones de la asamblea que deben propagarse a otros m�dulos del sistema.

| Campo | Tipo de Dato | Obligatorio | Descripci�n / Reglas |
|-------|--------------|-------------|----------------------|
| **Asamblea (`assemblyId`)** | `Guid` FK | S� | Asamblea que tom� la decisi�n. Cascade delete. |
| **Punto del Orden del D�a (`agendaItemId`)** | `Guid` FK | S� | Punto que origin� la decisi�n. Cascade delete. |
| **M�dulo Destino (`targetModule`)** | `Enum` string | S� | `Budget` � `ExtraordinaryFee` � `AuthRoles` � `AccountingEntry` � `Contract` � `Other`. |
| **Estado (`status`)** | `Enum` string | S� | `Pending` � `Propagated` � `Failed`. |
| **Descripci�n (`description`)** | `string` max 2000 | S� | Acci�n a ejecutar en el m�dulo destino. |
| **ID Entidad Destino (`targetEntityId`)** | `string` max 100 | No | ID de la entidad creada/actualizada en el m�dulo destino. |
| **Tipo Entidad Destino (`targetEntityType`)** | `string` max 100 | No | Tipo de entidad destino. Ej. "ExtraordinaryFee". |
| **Error (`errorMessage`)** | `string` max 4000 | No | Mensaje de error si la propagaci�n fall�. |
| **Reintentos (`retryCount`)** | `int` | Autom�tico | N�mero de intentos de propagaci�n. |
| **Propagada El (`propagatedAt`)** | `datetime` | No | Fecha de propagaci�n exitosa. |
| **Propagada Por (`propagatedByUserId`)** | `string` max 450 | No | ID del usuario o servicio que propag�. |

> [!NOTE]
> Al aprobar un punto de agenda con `DecisionPropagationTarget`, el motor intenta ejecutar la acci�n en el m�dulo destino. Si falla, queda en `Pending` para reintento o intervenci�n manual.

### 14.8 Reglas de Negocio del M�dulo

1. **Convocatoria**: Debe enviarse con al menos **10 d�as h�biles** para Asamblea Ordinaria y **7 d�as h�biles** para Extraordinaria (Ley 675 Arts. 37-38).

2. **C�lculo de quorum**: `totalCoefficients` = suma de coeficientes de unidades activas al convocar. `quorumThresholdFirstCall = totalCoefficients / 2`. `quorumThresholdSecondCall = totalCoefficients x 0.25`.

3. **Votaci�n por coeficiente**: Cada unidad tiene tantos votos como su coeficiente. Decisiones se toman por mayor�a de coeficientes, no por n�mero de asistentes.

4. **Restricci�n de voto por mora**: Propietarios con deudas vencidas no pueden votar (Ley 675 Art. 60). La restricci�n puede levantarse manualmente si el propietario se pone al d�a.

5. **Mayor�as**: `Simple` = sobre coeficientes representados. `Qualified` (70%) = sobre coeficiente total del conjunto. `Unanimity` = 100%.

6. **Actas**: El acta generada autom�ticamente incluye datos de la asamblea, lista de asistentes, resultados de votaci�n y constancias. Una vez publicada es **inmutable**.

7. **Propagaci�n autom�tica**: Aprobaci�n de presupuesto -> activa presupuesto en m�dulo contable. Cuota extraordinaria -> genera distribuciones en cartera. Cambios en administraci�n -> actualiza roles de acceso.

---

## 15. M�dulo de Reservas de Zonas Comunes

Gesti�n integral de la reserva de espacios comunes (salones sociales, piscinas, canchas, parques infantiles, BBQ) por parte de los propietarios y residentes. Incluye disponibilidad por horario, dep�sitos de garant�a, control de acceso, registro de incidentes y recordatorios autom�ticos.

> [!IMPORTANT]
> Cada espacio tiene reglas configurables: horario de operaci�n, capacidad m�xima, anticipaci�n m�nima/m�xima, pol�tica de mora, modo de aprobaci�n y costo asociado.

### 15.1 Espacio Reservable (`ReservableSpace`) � Tabla: `erp_reservable_spaces`

Cat�logo de espacios comunes que los residentes pueden reservar. Hereda de `BaseEntity` (soft delete).

| Campo | Tipo de Dato | Obligatorio | Descripci�n / Reglas |
|-------|--------------|-------------|----------------------|
| **Nombre (`name`)** | `string` max 300 | S� | Nombre del espacio. Ej. "Sal�n Social", "Piscina". |
| **Descripci�n (`description`)** | `string` max 2000 | No | Descripci�n del espacio, servicios incluidos y condiciones de uso. |
| **Ubicaci�n (`location`)** | `string` max 300 | No | Ubicaci�n f�sica dentro del conjunto. |
| **Capacidad M�xima (`maxCapacity`)** | `int` | S� | N�mero m�ximo de personas permitidas. |
| **Horas M�nimas Reserva (`minReservationHours`)** | `int` | S� | Duraci�n m�nima en horas. Default: 1. |
| **Horas M�ximas Reserva (`maxReservationHours`)** | `int` | S� | Duraci�n m�xima en horas. Default: 8. |
| **Anticipaci�n M�nima (`minAdvanceHours`)** | `int` | S� | Horas m�nimas de anticipaci�n. Default: 2. |
| **Anticipaci�n M�xima (`maxAdvanceDays`)** | `int` | S� | D�as m�ximos de anticipaci�n. Default: 30. |
| **M�x. Reservas Simult�neas (`maxSimultaneousReservationsPerUnit`)** | `int` | S� | M�ximo de reservas activas por unidad al mismo tiempo. Default: 2. |
| **Requiere Dep�sito (`requiresDeposit`)** | `boolean` | S� | `true` = requiere dep�sito de garant�a. |
| **Monto Dep�sito (`depositAmount`)** | `decimal(18,2)` | Cond. | Obligatorio si `requiresDeposit = true`. |
| **Tiene Costo Adicional (`hasAdditionalCost`)** | `boolean` | S� | `true` = el uso del espacio tiene costo. |
| **Tipo de Cobro (`chargeType`)** | `Enum` string | Cond. | `PerHour` = Por hora � `PerEvent` = Por evento. |
| **Tarifa por Hora (`hourlyRate`)** | `decimal(18,2)` | Cond. | Obligatorio si `chargeType = PerHour`. |
| **Tarifa por Evento (`eventRate`)** | `decimal(18,2)` | Cond. | Obligatorio si `chargeType = PerEvent`. |
| **Modo de Aprobaci�n (`approvalMode`)** | `Enum` string | S� | `Automatic` = Autom�tica � `Manual` = Requiere revisi�n del administrador. |
| **Pol�tica de Mora (`arrearsPolicy`)** | `Enum` string | S� | `Block` = Bloquear si en mora � `Warn` = Advertir pero permitir. |
| **Disponible para Mantenimiento (`isAvailableForMaintenance`)** | `boolean` | S� | `true` = disponible para bloqueos por mantenimiento. |
| **Activo (`isActive`)** | `boolean` | S� | `false` = espacio desactivado. |
| **Ruta Reglamento (`rulesFilePath`)** | `string` max 500 | No | Ruta del PDF con reglamento de uso. |
| **Ruta Imagen (`imageFilePath`)** | `string` max 500 | No | Ruta de imagen representativa. |
| **Creado Por (`createdByUserId`)** | `string` max 450 | Autom�tico | ID del usuario que cre� el espacio. |
| **Actualizado Por (`updatedByUserId`)** | `string` max 450 | No | ID del �ltimo usuario que modific� el espacio. |

> [!NOTE]
> **�ndices**: `(TenantId, Name)` �nico � `(TenantId, IsActive)`. Soft delete aplicado. Todos los hijos (horarios, bloques, reservas) se eliminan en cascada.

### 15.2 Horario del Espacio (`SpaceSchedule`) � Tabla: `erp_space_schedules`

Define los d�as y horas de operaci�n de cada espacio.

| Campo | Tipo de Dato | Obligatorio | Descripci�n / Reglas |
|-------|--------------|-------------|----------------------|
| **Espacio (`spaceId`)** | `Guid` FK | S� | Espacio al que pertenece el horario. Cascade delete. |
| **D�a de la Semana (`dayOfWeek`)** | `int` | S� | `0` = Domingo � `1` = Lunes � ... � `6` = S�bado. |
| **Hora Inicio (`startTime`)** | `string` max 10 | S� | Hora de apertura en formato HH:mm. |
| **Hora Fin (`endTime`)** | `string` max 10 | S� | Hora de cierre en formato HH:mm. |
| **Activo (`isActive`)** | `boolean` | S� | `false` si el horario est� desactivado temporalmente. |

> [!NOTE]
> �ndices: `(TenantId, SpaceId)` � `(TenantId, SpaceId, DayOfWeek)`.

### 15.3 Bloqueo del Espacio (`SpaceBlock`) � Tabla: `erp_space_blocks`

Per�odos en los que un espacio no est� disponible para reservas (mantenimiento, eventos administrativos, emergencias).

| Campo | Tipo de Dato | Obligatorio | Descripci�n / Reglas |
|-------|--------------|-------------|----------------------|
| **Espacio (`spaceId`)** | `Guid` FK | S� | Espacio bloqueado. Cascade delete. |
| **Fecha Inicio (`startDate`)** | `datetime` | S� | Inicio del per�odo de bloqueo. |
| **Fecha Fin (`endDate`)** | `datetime` | S� | Fin del per�odo de bloqueo. |
| **Hora Inicio (`startTime`)** | `string` max 10 | S� | Hora de inicio del bloqueo. |
| **Hora Fin (`endTime`)** | `string` max 10 | S� | Hora de fin del bloqueo. |
| **Origen (`origin`)** | `Enum` string | S� | `Maintenance` = Mantenimiento � `Administrative` = Administrativo � `Emergency` = Emergencia � `Other` = Otro. |
| **Motivo (`reason`)** | `string` max 1000 | No | Raz�n del bloqueo. |
| **Orden de Trabajo Relacionada (`relatedWorkOrderId`)** | `Guid?` | No | Referencia a la orden de trabajo si el bloqueo es por mantenimiento. |
| **Nro. Orden (`relatedWorkOrderNumber`)** | `string` max 100 | No | N�mero de la orden de trabajo relacionada. |
| **Notificar Afectados (`notifyAffectedResidents`)** | `boolean` | S� | `true` = notificar a residentes con reservas afectadas. |
| **Notificaci�n Enviada (`notificationSent`)** | `boolean` | S� | `true` si ya se envi� la notificaci�n. |
| **Creado Por (`createdByUserId`)** | `string` max 450 | Autom�tico | ID del usuario que cre� el bloqueo. |

> [!IMPORTANT]
> Al crear un bloqueo, el sistema detecta autom�ticamente las reservas existentes en el per�odo afectado y, si `notifyAffectedResidents = true`, env�a notificaciones de cancelaci�n a los residentes afectados.

### 15.4 Reserva (`Reservation`) � Tabla: `erp_reservations`

Registro de cada reserva realizada por un propietario o residente para usar un espacio com�n.

| Campo | Tipo de Dato | Obligatorio | Descripci�n / Reglas |
|-------|--------------|-------------|----------------------|
| **N�mero de Reserva (`reservationNumber`)** | `string` max 50 | Autom�tico | Formato `RES-000001`. Secuencia autoincremental por tenant. |
| **Espacio (`spaceId`)** | `Guid` FK | S� | Espacio reservado. FK a `erp_reservable_spaces` (Restrict). |
| **Unidad (`unitId`)** | `Guid` FK | S� | Unidad que realiza la reserva. FK a `erp_units` (Restrict). |
| **Propietario (`ownerId`)** | `Guid` FK | S� | Propietario responsable. FK a `erp_owners` (Restrict). |
| **Fecha/Hora Inicio (`startDateTime`)** | `datetime` | S� | Inicio de la reserva. |
| **Fecha/Hora Fin (`endDateTime`)** | `datetime` | S� | Fin de la reserva. Debe ser posterior a `startDateTime`. |
| **Asistentes Estimados (`estimatedAttendees`)** | `int` | S� | N�mero estimado de asistentes. No puede exceder `maxCapacity` del espacio. |
| **Descripci�n del Evento (`eventDescription`)** | `string` max 2000 | No | Motivo o descripci�n del evento. |
| **Tiene M�sica (`hasMusic`)** | `boolean` | S� | `true` si el evento incluye equipo de sonido. |
| **Hora Fin M�sica (`musicEndTime`)** | `string` max 10 | Cond. | Obligatorio si `hasMusic = true`. Hora l�mite para m�sica. |
| **Reglamento Aceptado (`rulesAccepted`)** | `boolean` | S� | `true` = el residente acept� el reglamento de uso del espacio. |
| **Estado (`status`)** | `Enum` string | S� | `Requested` = Solicitada � `Approved` = Aprobada � `Rejected` = Rechazada � `Cancelled` = Cancelada � `InUse` = En uso � `Completed` = Completada � `WithIncident` = Con incidente. |
| **Motivo Rechazo (`rejectionReason`)** | `string` max 1000 | Cond. | Obligatorio si `status = Rejected`. |
| **Costo Total (`totalCost`)** | `decimal(18,2)` | Autom�tico | Calculado seg�n `chargeType` del espacio. |
| **Estado Dep�sito (`depositStatus`)** | `Enum` string | S� | `NotRequired` � `Pending` � `Paid` � `Returned` � `AppliedToDamage`. |
| **Monto Dep�sito (`depositAmount`)** | `decimal(18,2)` | Autom�tico | Copia del `depositAmount` del espacio al momento de la reserva. |
| **Notas Admin (`adminNotes`)** | `string` max 2000 | No | Notas internas de la administraci�n. |
| **Admin a Cargo (`adminUserId`)** | `string` max 450 | No | ID del administrador que gestion� la reserva. |
| **Check-In (`checkedInAt`)** | `datetime` | No | Hora real de ingreso al espacio. |
| **Check-Out (`checkedOutAt`)** | `datetime` | No | Hora real de salida. |
| **Ruta Firma Checkout (`checkoutSignaturePath`)** | `string` max 500 | No | Ruta de la firma digital del residente al retirarse. |
| **Excepci�n Concedida (`exceptionGranted`)** | `boolean` | S� | `true` si se concedi� una excepci�n a las reglas del espacio. |
| **Motivo Excepci�n (`exceptionReason`)** | `string` max 1000 | Cond. | Raz�n de la excepci�n. |
| **Excepci�n Concedida Por (`exceptionGrantedByUserId`)** | `string` max 450 | Cond. | ID del administrador que concedi� la excepci�n. |
| **Creado Por (`createdByUserId`)** | `string` max 450 | Autom�tico | ID del usuario que cre� la reserva. |
| **Actualizado Por (`updatedByUserId`)** | `string` max 450 | No | ID del �ltimo usuario que modific� la reserva. |

> [!IMPORTANT]
> **Validaciones**: El motor de disponibilidad verifica: (1) que el espacio est� activo, (2) que la fecha/hora est� dentro del horario de operaci�n, (3) que no haya bloqueos ni reservas que se solapen, (4) que no se exceda `maxSimultaneousReservationsPerUnit`, (5) que la unidad no est� en mora si la pol�tica es `Block`. Si `approvalMode = Automatic` y todas las validaciones pasan, la reserva se crea en estado `Approved`.

### 15.5 Dep�sito de Garant�a (`ReservationDeposit`) � Tabla: `erp_reservation_deposits`

Registro de dep�sitos de garant�a asociados a una reserva.

| Campo | Tipo de Dato | Obligatorio | Descripci�n / Reglas |
|-------|--------------|-------------|----------------------|
| **Reserva (`reservationId`)** | `Guid` FK | S� | Reserva asociada. Cascade delete. |
| **Monto (`amount`)** | `decimal(18,2)` | S� | Valor del dep�sito. |
| **Estado (`status`)** | `Enum` string | S� | `Pending` � `Paid` � `Returned` � `AppliedToDamage`. |
| **M�todo de Pago (`paymentMethod`)** | `Enum` string | Cond. | `Cash` � `BankTransfer` � `CreditCard` � `DebitCard` � `OnlinePayment` � `AppliedToAccount`. |
| **ID Cobro (`chargeId`)** | `Guid?` | No | Referencia al cobro individual generado por el dep�sito. |
| **Nro. Cobro (`chargeNumber`)** | `string` max 100 | No | N�mero del cobro generado. |
| **ID Devoluci�n (`returnChargeId`)** | `Guid?` | No | Referencia al cobro de devoluci�n del dep�sito. |
| **Nro. Devoluci�n (`returnChargeNumber`)** | `string` max 100 | No | N�mero del cobro de devoluci�n. |
| **Monto Da�o (`damageAmount`)** | `decimal(18,2)` | No | Monto aplicado a da�os. |
| **Descripci�n Da�o (`damageDescription`)** | `string` max 2000 | Cond. | Descripci�n de los da�os si `status = AppliedToDamage`. |
| **Pagado El (`paidAt`)** | `datetime` | No | Fecha de pago del dep�sito. |
| **Devuelto El (`returnedAt`)** | `datetime` | No | Fecha de devoluci�n. |
| **Aplicado El (`appliedAt`)** | `datetime` | No | Fecha de aplicaci�n a da�os. |
| **Procesado Por (`processedByUserId`)** | `string` max 450 | No | ID del usuario que proces� el dep�sito. |
| **Notas (`notes`)** | `string` max 2000 | No | Observaciones sobre el dep�sito. |

### 15.6 Incidente de Reserva (`ReservationIncident`) � Tabla: `erp_reservation_incidents`

Registro de incidentes ocurridos durante el uso de un espacio reservado.

| Campo | Tipo de Dato | Obligatorio | Descripci�n / Reglas |
|-------|--------------|-------------|----------------------|
| **Reserva (`reservationId`)** | `Guid` FK | S� | Reserva asociada. Cascade delete. |
| **Descripci�n (`description`)** | `string` max 4000 | S� | Detalle del incidente. |
| **Gravedad (`severity`)** | `Enum` string | S� | `Minor` = Leve � `Moderate` = Moderado � `Severe` = Grave � `Critical` = Cr�tico. |
| **Monto Da�o (`damageAmount`)** | `decimal(18,2)` | S� | Valor estimado del da�o. |
| **Da�o Evaluado (`damageAssessed`)** | `boolean` | S� | `true` si el da�o fue evaluado formalmente. |
| **Dep�sito Aplicado (`depositAppliedToDamage`)** | `boolean` | S� | `true` si el dep�sito se aplic� al da�o. |
| **Ruta Evidencia (`evidenceFilePath`)** | `string` max 500 | No | Ruta del archivo de evidencia (foto/video). |
| **Reportado Por (`reportedByUserId`)** | `string` max 450 | Autom�tico | ID del usuario que report� el incidente. |
| **Nombre Reportante (`reportedByName`)** | `string` max 300 | No | Nombre de la persona que report�. |

> [!IMPORTANT]
> Al registrar un incidente con `severity = Severe` o `Critical`, el sistema notifica autom�ticamente al administrador y al consejo. Si `depositAppliedToDamage = true`, se actualiza el estado del dep�sito de la reserva.

### 15.7 Recordatorio de Reserva (`ReservationReminder`) � Tabla: `erp_reservation_reminders`

Recordatorios autom�ticos enviados a los residentes antes de su reserva.

| Campo | Tipo de Dato | Obligatorio | Descripci�n / Reglas |
|-------|--------------|-------------|----------------------|
| **Reserva (`reservationId`)** | `Guid` FK | S� | Reserva asociada. Cascade delete. |
| **Tipo (`reminderType`)** | `Enum` string | S� | `TwentyFourHours` = 24h antes � `TwoHours` = 2h antes � `Custom` = Personalizado. |
| **Estado (`status`)** | `Enum` string | S� | `Pending` � `Sent` � `Failed`. |
| **Programado Para (`scheduledFor`)** | `datetime` | S� | Fecha/hora programada para el env�o. |
| **Enviado El (`sentAt`)** | `datetime` | No | Fecha/hora real de env�o. |
| **Canal (`channel`)** | `string` max 30 | S� | Canal de env�o: `Email`, `Sms` o `Push`. |
| **Email Destino (`recipientEmail`)** | `string` max 300 | No | Email del destinatario (snapshot). |
| **Tel�fono Destino (`recipientPhone`)** | `string` max 50 | No | Tel�fono del destinatario (snapshot). |
| **Error (`errorMessage`)** | `string` max 1000 | No | Mensaje de error si fall� el env�o. |
| **Reintentos (`retryCount`)** | `int` | Autom�tico | N�mero de reintentos. |

> [!NOTE]
> El motor `ReservationReminderEngine` ejecuta peri�dicamente y env�a los recordatorios pendientes cuya `scheduledFor <= now`.

### 15.8 Reglas de Negocio del M�dulo

1. **Disponibilidad**: El motor `ReservationAvailabilityEngine` verifica superposici�n de horarios contra reservas existentes en estados `Requested`, `Approved` o `InUse`, y contra todos los bloqueos activos.

2. **Aprobaci�n autom�tica**: Si `approvalMode = Automatic` y todas las validaciones pasan, la reserva se crea en estado `Approved`. Si `approvalMode = Manual`, queda en `Requested` pendiente de revisi�n del administrador.

3. **Check-in y Check-out**: El administrador registra el ingreso (`checkedInAt`) y salida (`checkedOutAt`) del residente. A la salida, se inspecciona el espacio. Si hay da�os, se registra un incidente.

4. **Dep�sito de garant�a**: Si `requiresDeposit = true`, la reserva no puede pasar a `Approved` hasta que el dep�sito est� en estado `Paid`. El dep�sito se devuelve despu�s del check-out exitoso o se aplica a da�os si aplica.

5. **Pol�tica de mora**: Con `arrearsPolicy = Block`, el sistema verifica que la unidad no tenga deuda vencida al momento de crear la reserva. Con `Warn`, se muestra una advertencia pero se permite la reserva.

6. **L�mite de reservas simult�neas**: Una unidad no puede tener m�s de `maxSimultaneousReservationsPerUnit` reservas en estados `Approved` o `InUse` al mismo tiempo en el mismo espacio.

7. **Bloqueos por mantenimiento**: Los bloqueos se crean autom�ticamente cuando se genera una orden de trabajo preventivo que afecta un espacio. El sistema notifica a los residentes con reservas afectadas.

8. **Costos**: Si `hasAdditionalCost = true`, el costo total se calcula seg�n el tipo de cobro. `totalCost` se registra en la reserva y puede integrarse con el m�dulo de cartera como un cobro individual.
