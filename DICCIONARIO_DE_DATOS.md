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
| **Identificador (`identifier`)** | `string` | Sí | Nombre de la unidad tal como se conoce en el conjunto. Ej. "A-101", "Casa 4", "Local 3B". |
| **Tipo de Unidad (`unitTypeName`)** | `string` | Sí | Clasificación arquitectónica. Ej. "Apartment", "House", "Commercial Locale". |
| **Torre o Bloque (`towerOrBlock`)** | `string` | Sí | Agrupación física de la unidad. Ej. "Torre 1", "Bloque A". |
| **Nivel o Piso (`floorLevel`)** | `int` | Sí | Piso en el que se ubica. Si es casa de un solo nivel, poner `1`. |
| **Área Privada (`privateArea`)** | `decimal` | Sí | Área construida o privada en metros cuadrados (m²). |
| **Área de Balcón (`balconyArea`)** | `decimal` | Sí | Área del balcón o terraza en metros cuadrados (m²). Poner `0` si no aplica. |
| **Coeficiente (`coproprietyCoefficient`)** | `decimal` | Sí | Porcentaje de participación. Este valor dicta el cobro de la cuota de administración y los votos en asamblea. La sumatoria global debe dar `100.00`. |
| **Estado (`status`)** | `Enum` (int) | Sí | Estado físico de ocupación. Valores posibles: <br>• `1` Activa y Ocupada <br>• `2` Activa y Desocupada <br>• `3` En Proceso de Entrega <br>• `4` En Litigio <br>• `5` Inactiva |
| **Tiene Parqueadero Privado (`hasPrivateParking`)**| `boolean` | Sí | Marca si la unidad tiene un parqueadero asignado por escritura pública. |
| **Identificador Parqueadero (`parkingIdentifier`)** | `string` | No | Si la anterior es `true`, debe especificarse qué parqueadero es (Ej. "P-23"). |
| **Tiene Cuarto Útil (`hasAssignedStorage`)** | `boolean` | Sí | Marca si la unidad tiene bodega/cuarto útil o depósito asignado. |
| **Identificador Cuarto Útil (`storageIdentifier`)** | `string` | No | Si la anterior es `true`, especificar el número de la bodega (Ej. "B-12"). |
| **Observaciones Internas (`internalObservations`)** | `string` | No | Notas administrativas no visibles por los residentes sobre la unidad. |

---

*Nota: Este archivo se irá expandiendo conforme se desarrollen nuevos módulos como CRM (Residentes, Mascotas, Vehículos) y Finanzas (Recaudos, Cartera).*
