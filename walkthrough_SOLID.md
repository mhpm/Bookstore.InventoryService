# Guía Detallada: SOLID aplicado al Inventory Service (Backend - C#)

Esta guía detalla los principios de diseño **SOLID** que hemos implementado en el microservicio de inventario (`Bookstore.InventoryService`), indicando los archivos modificados, dónde se aplican y la justificación técnica de cada decisión para que sirva como material educativo.

---

## 1. Single Responsibility Principle (SRP) - Principio de Responsabilidad Única

> **Definición:** Una clase debe tener una, y solo una, razón para cambiar.

### ¿Dónde se aplica?
*   **[OrderCreatedConsumer.cs](file:///c:/Users/miche/Desktop/Bookstore/Bookstore.InventoryService/src/InventoryService/Messaging/OrderCreatedConsumer.cs):** Refactorizado para actuar únicamente como un adaptador o puente de mensajería (MassTransit). Recibe el evento y delega el procesamiento de negocio. Ya no manipula la base de datos directamente.
*   **[BookStockService.cs](file:///c:/Users/miche/Desktop/Bookstore/Bookstore.InventoryService/src/InventoryService/Services/BookStockService.cs) (NUEVO):** Es la clase responsable únicamente de hacer cumplir las reglas de negocio asociadas con el stock del inventario.
*   **[BookRepository.cs](file:///c:/Users/miche/Desktop/Bookstore/Bookstore.InventoryService/src/InventoryService/Data/Repositories/BookRepository.cs) (NUEVO):** Responsable únicamente de la persistencia de datos (operaciones CRUD y query mappings).

### ¿Por qué?
Antes, el consumidor `OrderCreatedConsumer` hacía de todo: procesaba el mensaje de RabbitMQ, buscaba los libros en la base de datos, restaba el stock de manera directa, registraba en logs y guardaba los cambios. Si cambiaba la estructura de mensajería o si cambiaba la base de datos, el archivo tenía que ser modificado. Ahora, cada clase tiene una única responsabilidad bien definida.

---

## 2. Open/Closed Principle (OCP) - Principio de Abierto/Cerrado

> **Definición:** Las entidades de software deben estar abiertas para su extensión, pero cerradas para su modificación.

### ¿Dónde se aplica?
*   **[BookStockService.cs](file:///c:/Users/miche/Desktop/Bookstore/Bookstore.InventoryService/src/InventoryService/Services/BookStockService.cs):** La lógica de descuento de stock de libros está encapsulada en esta clase.
*   **[BookRepository.cs](file:///c:/Users/miche/Desktop/Bookstore/Bookstore.InventoryService/src/InventoryService/Data/Repositories/BookRepository.cs):** La función de base de datos `fn_CalculateDiscount` está oculta detrás de `GetDiscountedAsync`.

### ¿Por qué?
Si en el futuro deseamos añadir reglas complejas al reducir stock (por ejemplo: enviar un correo si el stock baja de 5 unidades, o bloquear compras si el stock es negativo), podemos extender esa funcionalidad dentro de `BookStockService` sin alterar en absoluto a `OrderCreatedConsumer` (mensajería) ni a `BooksController` (API HTTP).
Igualmente, si la forma física en que la base de datos calcula los descuentos cambia, modificamos la consulta en `BookRepository`, pero los Handlers de lectura de MediatR permanecen cerrados a la modificación.

---

## 3. Liskov Substitution Principle (LSP) - Principio de Sustitución de Liskov

> **Definición:** Las clases derivadas o implementaciones deben ser sustituibles por sus tipos base sin alterar el comportamiento correcto del programa.

### ¿Dónde se aplica?
En el uso de **[FakeBookRepository](file:///c:/Users/miche/Desktop/Bookstore/Bookstore.InventoryService/tests/InventoryService.Tests/BookHandlersTests.cs#L206)** dentro de nuestro proyecto de pruebas unitarias.

### ¿Por qué?
Dado que los Handlers de MediatR (ej. `GetDiscountedBooksQueryHandler`) dependen de la abstracción `IBookRepository`, pudimos sustituir la clase real `BookRepository` (que se conecta a PostgreSQL a través de EF Core y requiere la función SQL `fn_CalculateDiscount`) por la clase `FakeBookRepository` (que usa una lista en memoria y calcula el descuento en C#).
Los Handlers compilaron y ejecutaron sus pruebas de forma transparente. Esto demuestra que `FakeBookRepository` es perfectamente sustituible por `BookRepository` (LSP) sin corromper el sistema.

---

## 4. Interface Segregation Principle (ISP) - Principio de Segregación de Interfaces

> **Definición:** Los clientes no deben ser obligados a depender de interfaces que no utilizan.

### ¿Dónde se aplica?
En la creación de interfaces cohesivas e independientes:
*   **[IBookRepository.cs](file:///c:/Users/miche/Desktop/Bookstore/Bookstore.InventoryService/src/InventoryService/Data/Repositories/IBookRepository.cs):** Define exclusivamente contratos de base de datos y persistencia.
*   **[IBookStockService.cs](file:///c:/Users/miche/Desktop/Bookstore/Bookstore.InventoryService/src/InventoryService/Services/IBookStockService.cs):** Define exclusivamente el contrato de reducción de existencias físicas de inventario.

### ¿Por qué?
El consumidor de eventos `OrderCreatedConsumer` no necesita saber cómo agregar libros, borrarlos, o listar libros con descuentos (operaciones CRUD). Al separar los contratos, el consumidor solo depende de `IBookStockService` y su único método `ReduceStockAsync`, evitando cargar con métodos innecesarios de base de datos.

---

## 5. Dependency Inversion Principle (DIP) - Principio de Inversión de Dependencias

> **Definición:** Los módulos de alto nivel no deben depender de módulos de bajo nivel. Ambos deben depender de abstracciones.

### ¿Dónde se aplica?
En el desacoplamiento de todos los comandos y consultas de MediatR con respecto al DbContext de Entity Framework:
*   Los handlers de consultas ([GetBooks](file:///c:/Users/miche/Desktop/Bookstore/Bookstore.InventoryService/src/InventoryService/Features/Books/Queries/GetBooks.cs)) y comandos ([CreateBook](file:///c:/Users/miche/Desktop/Bookstore/Bookstore.InventoryService/src/InventoryService/Features/Books/Commands/CreateBook.cs)) ya no inyectan `InventoryDbContext`.
*   En su lugar, inyectan la abstracción `IBookRepository`.
*   El contenedor de dependencias IoC en **[Program.cs](file:///c:/Users/miche/Desktop/Bookstore/Bookstore.InventoryService/src/InventoryService/Program.cs)** se encarga de resolver la instancia en tiempo de ejecución:
    ```csharp
    builder.Services.AddScoped<IBookRepository, BookRepository>();
    builder.Services.AddScoped<IBookStockService, BookStockService>();
    ```

### ¿Por qué?
Antes, los componentes de aplicación (Handlers) dependían directamente de Entity Framework Core (detalles de bajo nivel). Si queríamos migrar a Dapper, ADO.NET o MongoDB, tendríamos que reescribir todos los Handlers. Ahora, los Handlers dependen de la abstracción `IBookRepository`. Cambiar la tecnología de acceso a datos solo requiere crear una nueva implementación de la interfaz, dejando intactos los Handlers de negocio.
