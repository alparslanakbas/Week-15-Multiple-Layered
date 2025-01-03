namespace Multiple_Layered_DataAccess.Library.EntityConfigurations
{
    internal class OrderEntityConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.OrderDate)
                .IsRequired()
                .HasComment("Sipariş Tarihi");

            builder.Property(x => x.TotalAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasComment("Toplam Tutar");

            builder.HasOne<User>()
                .WithMany(x => x.Orders)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Orders_Users");

            builder.ToTable("Orders", x => x.HasComment("Siparişlerin Tutulduğu Tablo"));

        }
    }
}
