using iText.Kernel.Font;
namespace UserService.Infrastructure.Configurations.Fonts.interfaces
{
    public interface IFontProvider
    {
        PdfFont CreateUniversalFont();
    }
}