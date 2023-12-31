﻿// <auto-generated />
using System;
using Banco.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Banco.Migrations
{
    [DbContext(typeof(MiContexto))]
    [Migration("20230808023459_Inicial")]
    partial class Inicial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0-preview.6.23329.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Banco.Models.CajaDeAhorro", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("id"));

                    b.Property<string>("TitularApellido")
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("TitularNombre")
                        .HasColumnType("nvarchar(50)");

                    b.Property<int?>("cbu")
                        .IsRequired()
                        .HasColumnType("int");

                    b.Property<int>("idUsuario")
                        .HasColumnType("int");

                    b.Property<double>("saldo")
                        .HasColumnType("float");

                    b.HasKey("id");

                    b.ToTable("CajaAhorro", (string)null);
                });

            modelBuilder.Entity("Banco.Models.Movimiento", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("id"));

                    b.Property<string>("detalle")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<DateTime>("fecha")
                        .HasColumnType("DateTime");

                    b.Property<int>("id_Caja")
                        .HasColumnType("int");

                    b.Property<double>("monto")
                        .HasColumnType("float");

                    b.HasKey("id");

                    b.HasIndex("id_Caja");

                    b.ToTable("Movimiento", (string)null);
                });

            modelBuilder.Entity("Banco.Models.Pago", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("id"));

                    b.Property<int>("id_usuario")
                        .HasColumnType("int");

                    b.Property<string>("metodo")
                        .HasColumnType("varchar(50)");

                    b.Property<double>("monto")
                        .HasColumnType("float");

                    b.Property<string>("nombre")
                        .IsRequired()
                        .HasColumnType("varchar(50)");

                    b.Property<bool>("pagado")
                        .HasColumnType("bit");

                    b.HasKey("id");

                    b.HasIndex("id_usuario");

                    b.ToTable("Pago", (string)null);
                });

            modelBuilder.Entity("Banco.Models.PlazoFijo", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("id"));

                    b.Property<int?>("cbu")
                        .HasColumnType("int");

                    b.Property<DateTime>("fechaFin")
                        .HasColumnType("dateTime");

                    b.Property<DateTime>("fechaIni")
                        .HasColumnType("dateTime");

                    b.Property<int>("id_titular")
                        .HasColumnType("int");

                    b.Property<double>("monto")
                        .HasColumnType("float");

                    b.Property<bool>("pagado")
                        .HasColumnType("bit");

                    b.Property<double>("tasa")
                        .HasColumnType("float");

                    b.HasKey("id");

                    b.HasIndex("id_titular");

                    b.ToTable("PlazoFijo", (string)null);
                });

            modelBuilder.Entity("Banco.Models.Tarjeta", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("id"));

                    b.Property<int>("codigoV")
                        .HasColumnType("int");

                    b.Property<double>("consumo")
                        .HasColumnType("float");

                    b.Property<int>("id_titular")
                        .HasColumnType("int");

                    b.Property<double>("limite")
                        .HasColumnType("float");

                    b.Property<int>("numero")
                        .HasColumnType("int");

                    b.HasKey("id");

                    b.HasIndex("id_titular");

                    b.ToTable("Tarjeta", (string)null);
                });

            modelBuilder.Entity("Banco.Models.Usuario", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("id"));

                    b.Property<string>("apellido")
                        .IsRequired()
                        .HasColumnType("varchar(50)");

                    b.Property<bool>("bloqueado")
                        .HasColumnType("bit");

                    b.Property<int?>("dni")
                        .IsRequired()
                        .HasColumnType("int");

                    b.Property<int>("intentosFallidos")
                        .HasColumnType("int");

                    b.Property<bool>("isAdmin")
                        .HasColumnType("bit");

                    b.Property<string>("mail")
                        .IsRequired()
                        .HasColumnType("varchar(50)");

                    b.Property<string>("nombre")
                        .IsRequired()
                        .HasColumnType("varchar(50)");

                    b.Property<string>("password")
                        .IsRequired()
                        .HasColumnType("varchar(100)");

                    b.HasKey("id");

                    b.ToTable("Usuario", (string)null);

                    b.HasData(
                        new
                        {
                            id = 1,
                            apellido = "Suarez",
                            bloqueado = false,
                            dni = 1234,
                            intentosFallidos = 0,
                            isAdmin = true,
                            mail = "matias@suarez",
                            nombre = "Matias",
                            password = "111"
                        },
                        new
                        {
                            id = 2,
                            apellido = "Enriquez",
                            bloqueado = false,
                            dni = 1235,
                            intentosFallidos = 0,
                            isAdmin = true,
                            mail = "emilio@enriquez",
                            nombre = "Emilio",
                            password = "111"
                        },
                        new
                        {
                            id = 3,
                            apellido = "Lacoa",
                            bloqueado = false,
                            dni = 1236,
                            intentosFallidos = 0,
                            isAdmin = true,
                            mail = "marcos@lacoa",
                            nombre = "Marcos",
                            password = "111"
                        },
                        new
                        {
                            id = 4,
                            apellido = "Arrojo",
                            bloqueado = false,
                            dni = 1237,
                            intentosFallidos = 0,
                            isAdmin = true,
                            mail = "agustin@arrojo",
                            nombre = "Agustin",
                            password = "111"
                        });
                });

            modelBuilder.Entity("Banco.Models.UsuarioCaja", b =>
                {
                    b.Property<int>("idUsuario")
                        .HasColumnType("int");

                    b.Property<int>("idCaja")
                        .HasColumnType("int");

                    b.HasKey("idUsuario", "idCaja");

                    b.HasIndex("idCaja");

                    b.ToTable("UsuarioCaja");
                });

            modelBuilder.Entity("Banco.Models.Movimiento", b =>
                {
                    b.HasOne("Banco.Models.CajaDeAhorro", "caja")
                        .WithMany("movimientos")
                        .HasForeignKey("id_Caja")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("caja");
                });

            modelBuilder.Entity("Banco.Models.Pago", b =>
                {
                    b.HasOne("Banco.Models.Usuario", "usuario")
                        .WithMany("pagos")
                        .HasForeignKey("id_usuario")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("usuario");
                });

            modelBuilder.Entity("Banco.Models.PlazoFijo", b =>
                {
                    b.HasOne("Banco.Models.Usuario", "titular")
                        .WithMany("pf")
                        .HasForeignKey("id_titular")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("titular");
                });

            modelBuilder.Entity("Banco.Models.Tarjeta", b =>
                {
                    b.HasOne("Banco.Models.Usuario", "titular")
                        .WithMany("tarjetas")
                        .HasForeignKey("id_titular")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("titular");
                });

            modelBuilder.Entity("Banco.Models.UsuarioCaja", b =>
                {
                    b.HasOne("Banco.Models.CajaDeAhorro", "caja")
                        .WithMany("usuarioCajas")
                        .HasForeignKey("idCaja")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Banco.Models.Usuario", "usuario")
                        .WithMany("usuarioCajas")
                        .HasForeignKey("idUsuario")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("caja");

                    b.Navigation("usuario");
                });

            modelBuilder.Entity("Banco.Models.CajaDeAhorro", b =>
                {
                    b.Navigation("movimientos");

                    b.Navigation("usuarioCajas");
                });

            modelBuilder.Entity("Banco.Models.Usuario", b =>
                {
                    b.Navigation("pagos");

                    b.Navigation("pf");

                    b.Navigation("tarjetas");

                    b.Navigation("usuarioCajas");
                });
#pragma warning restore 612, 618
        }
    }
}
