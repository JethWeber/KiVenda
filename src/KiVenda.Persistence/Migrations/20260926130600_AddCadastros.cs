using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable
namespace KiVenda.Persistence.Migrations;
[Migration("20260926130600_AddCadastros")]
public partial class AddCadastros:Migration
{
 protected override void Up(MigrationBuilder m)
 {
  m.AddColumn<string>("Email","Clientes",type:"TEXT",maxLength:150,nullable:true);
  m.AddColumn<string>("Nif","Clientes",type:"TEXT",maxLength:30,nullable:true);
  m.AddColumn<string>("Email","Fornecedores",type:"TEXT",maxLength:150,nullable:true);
  m.AddColumn<string>("Nif","Fornecedores",type:"TEXT",maxLength:30,nullable:true);
  m.CreateTable("Funcionarios",
   columns:t=>new {Id=t.Column<Guid>("TEXT",nullable:false),CriadoEm=t.Column<DateTime>("TEXT",nullable:false),AtualizadoEm=t.Column<DateTime>("TEXT",nullable:true),Codigo=t.Column<string>("TEXT",maxLength:20,nullable:false),Nome=t.Column<string>("TEXT",maxLength:150,nullable:false),Telefone=t.Column<string>("TEXT",maxLength:30,nullable:true),Email=t.Column<string>("TEXT",maxLength:150,nullable:true),BI=t.Column<string>("TEXT",maxLength:40,nullable:true),Cargo=t.Column<string>("TEXT",maxLength:100,nullable:true),Departamento=t.Column<string>("TEXT",maxLength:100,nullable:true),Turno=t.Column<string>("TEXT",maxLength:80,nullable:true),DataAdmissao=t.Column<DateTime>("TEXT",nullable:false),SalarioBase=t.Column<decimal>("TEXT",precision:18,scale:2,nullable:false),Ativo=t.Column<bool>("INTEGER",nullable:false)},
   constraints:t=>t.PrimaryKey("PK_Funcionarios",x=>x.Id));
  m.CreateIndex("IX_Funcionarios_BI","Funcionarios","BI");
  m.CreateIndex("IX_Funcionarios_Codigo","Funcionarios","Codigo",unique:true);
  m.CreateIndex("IX_Funcionarios_Nome","Funcionarios","Nome");
  m.CreateIndex("IX_Clientes_Nif","Clientes","Nif");
  m.CreateIndex("IX_Fornecedores_Nif","Fornecedores","Nif");
 }
 protected override void Down(MigrationBuilder m){m.DropTable("Funcionarios");m.DropIndex("IX_Clientes_Nif","Clientes");m.DropIndex("IX_Fornecedores_Nif","Fornecedores");m.DropColumn("Email","Clientes");m.DropColumn("Nif","Clientes");m.DropColumn("Email","Fornecedores");m.DropColumn("Nif","Fornecedores");}
}