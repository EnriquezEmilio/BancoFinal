Documentación Final Plataformas de Desarrollo
Proyecto Banco - MVC 
Integrantes: Arrojo Agustín, Enriquez Emilio, Lacoa Marcos, Suárez Ibarra Matías.

En este trabajo, utilizando los modelos de vista-controlador, construímos un un home-banking para nuestro banco. En la siguiente documentación detallaremos
que se puede realizar en cada apartado del mismo. A lo largo de esta documentación, se examinarán en detalle los componentes esenciales del sistema, incluyendo
la descripción exhaustiva de los Modelos que encapsulan la lógica subyacente, las Vistas que presentan la información al usuario final y los Controladores que gestionan las interacciones entre el usuario y el sistema.

Además, se explorarán las funcionalidades clave que ofrece la aplicación Home-Banking, tales como la visualización de saldos, transferencias de fondos, pagos de facturas y otras
operaciones financieras relevantes. También se destacarán las medidas de seguridad implementadas para salvaguardar la integridad de los datos sensibles y la privacidad de los usuarios.

LoginController: Este controlador forma parte de la implementación del patrón MVC en el proyecto de Home-Banking, encargándose de manejar la autenticación, el 
inicio de sesión y el cierre de sesión de los usuarios, así como la gestión de errores.

RegistroController: En este controlador se permite crear un nuevo usuario, completando los campos de información personal. Para mayor seguridad e integridad de los datos, la
contraseña de hashea así no se puede ver desde la base de datos o el panel de administrador. 

MainController: Esta es la pestaña principal una vez logueado en la página. Desde acá se puede acceder a todas las funciones que ofrece la plataforma, ya sea Caja De Ahorro,
Tarjetas, Pagos y Plazo Fijos. Además, si el usuario es Administrador también verá el apartado de Usuarios. Al acceder a cada uno de los apartados, la página hará un sonido. 

UsuariosController: A esta pestaña solo se puede acceder siendo un administrador. En caso de no serlo, serás redirigido al main.Este controlador maneja las operaciones
de administración de usuarios, incluyendo la creación, edición, eliminación, bloqueo/desbloqueo y asignación de permisos de
administrador. Las acciones se realizan en base a la autenticación del usuario y su rol de administrador.

CajaDeAhorroesController: En general, este controlador maneja la lógica relacionada con la administración de cuentas de ahorro, incluyendo la
creación, edición, eliminación, depósito, retiro, transferencia y gestión de titulares (administrador), así como la visualización de movimientos. 
Cada método está diseñado para interactuar con la base de datos a través del contexto proporcionado y realizar las acciones correspondientes según la lógica de negocio definida.

TarjetasController: En este controlador se puede administrar todo lo relacionado a las tarjetas, ya sea su creación, edición o eliminación. También se pueden pagar.
Para la creación se utiliza un número aleatorio para el número de tarjeta y el cvv. 

PlazoFijoController: Este es el encargado de gestionar las operaciones de depósitos a plazo fijo en una aplicación bancaria. El controlador proporciona funcionalidades para crear, 
ver, editar y eliminar Plazos Fijos, así como para gestionar transacciones financieras relacionadas. Por nuestra parte incluimos tambien un boton para poder rescatar el plazo fijo en cualquier momento,
de hacer asi este se recupera pero sin interes. El controller utiliza la autenticación de usuarios y la gestión de sesiones para asociar acciones con usuarios que han iniciado sesión.
Proporciona vistas y funcionalidades diferentes según si el usuario es un administrador o un usuario regular.

PagosController: Este se encarga de la creación, visualización, edición, eliminacion y en caso de ser usuario regular tambien de los pagos. Los usuarios pueden manejar sus 
pagos, editarlos si es necesario y realizar transacciones seguras a través de diferentes métodos de pago, como cajas de ahorro y tarjetas. La lógica de validación y registro de transacciones 
financieras se integra perfectamente en el controlador, ofreciendo una experiencia sin inconvenientes tanto para administradores como para usuarios regulares.

