using KiVenda.Core.Funcionarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace KiVenda.Persistence.Configurations;
public sealed class FuncionarioConfiguration:IEntityTypeConfiguration<Funcionario>
{
 public void Configure(EntityTypeBuilder<Funcionario> builder){builder.ToTable("Funcionarios");builder.HasKey(x=>x.Id);builder.Property(x=>x.Codigo).IsRequired().HasMaxLength(20);builder.HasIndex(x=>x.Codigo).IsUnique();builder.Property(x=>x.Nome).IsRequired().HasMaxLength(150);builder.Property(x=>x.Telefone).HasMaxLength(30);builder.Property(x=>x.Email).HasMaxLength(150);builder.Property(x=>x.BI).HasMaxLength(40);builder.Property(x=>x.Cargo).HasMaxLength(100);builder.Property(x=>x.Departamento).HasMaxLength(100);builder.Property(x=>x.Turno).HasMaxLength(80);builder.Property(x=>x.SalarioBase).HasPrecision(18,2);builder.HasIndex(x=>x.Nome);builder.HasIndex(x=>x.BI);}
}