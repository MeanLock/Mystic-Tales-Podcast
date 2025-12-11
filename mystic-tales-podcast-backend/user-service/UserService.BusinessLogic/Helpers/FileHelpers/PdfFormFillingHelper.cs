using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Xobject;
using iText.Kernel.Geom;
using iText.Forms;
using iText.Forms.Fields;
using iText.IO.Image;
using iText.Layout;
using iText.Layout.Element;
using Microsoft.Extensions.Logging;

namespace UserService.BusinessLogic.Helpers.FileHelpers
{
    public class PdfFormFillingHelper
    {
        private readonly ILogger<PdfFormFillingHelper> _logger;

        public PdfFormFillingHelper(ILogger<PdfFormFillingHelper> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Fill PDF form fields and return filled PDF as byte array
        /// </summary>
        /// <param name="templateBytes">Template PDF bytes from S3</param>
        /// <param name="fieldValues">Dictionary of field names and values to fill</param>
        /// <param name="flattenForm">If true, form will be locked (non-editable)</param>
        /// <returns>Filled PDF as byte array</returns>
        public byte[] FillPdfTextFormFields(
            byte[] templateBytes,
            Dictionary<string, string> fieldValues,
            bool flattenForm = true)
        {
            MemoryStream outputStream = null;
            PdfReader reader = null;
            PdfWriter writer = null;
            PdfDocument pdfDoc = null;

            try
            {
                // Create streams
                var inputStream = new MemoryStream(templateBytes);
                outputStream = new MemoryStream();

                // Configure reader - IMPORTANT: Set to append mode for form filling
                reader = new PdfReader(inputStream);

                // Configure writer with proper properties
                var writerProperties = new WriterProperties();
                // Don't use smart mode for form filling
                // writerProperties.SetFullCompressionMode(true); // Optional compression

                writer = new PdfWriter(outputStream, writerProperties);

                // Open PDF in APPEND mode to preserve form structure
                var readerProperties = new ReaderProperties();
                pdfDoc = new PdfDocument(reader, writer);

                // Get the form
                var form = PdfAcroForm.GetAcroForm(pdfDoc, true);

                if (form == null)
                {
                    _logger.LogWarning("PDF template does not contain form fields");
                    throw new InvalidOperationException("PDF template does not contain form fields");
                }

                // Log available fields for debugging
                var availableFields = form.GetAllFormFields();
                _logger.LogInformation($"Found {availableFields.Count} form fields in PDF");

                // Fill each field
                int filledCount = 0;
                foreach (var fieldEntry in fieldValues)
                {
                    string fieldName = fieldEntry.Key;
                    string fieldValue = fieldEntry.Value ?? "";

                    if (availableFields.ContainsKey(fieldName))
                    {
                        try
                        {
                            var field = availableFields[fieldName];
                            field.SetValue(fieldValue);
                            filledCount++;
                            _logger.LogInformation($"✓ Filled field '{fieldName}' with value '{fieldValue}'");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"Failed to fill field '{fieldName}': {ex.Message}");
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"✗ Field '{fieldName}' not found in PDF template");
                    }
                }

                _logger.LogInformation($"Successfully filled {filledCount}/{fieldValues.Count} fields");

                // Flatten form to make it non-editable (optional)
                if (flattenForm)
                {
                    form.FlattenFields();
                    _logger.LogInformation("Form flattened (locked)");
                }

                // IMPORTANT: Close in correct order
                pdfDoc.Close(); // This also closes reader and writer

                byte[] result = outputStream.ToArray();
                _logger.LogInformation($"Generated PDF: {result.Length} bytes");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error filling PDF form: {ex.GetType().Name} - {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                throw new InvalidOperationException($"Failed to fill PDF form: {ex.Message}", ex);
            }
            finally
            {
                // Clean up - only dispose what wasn't closed by pdfDoc.Close()
                outputStream?.Dispose();
            }
        }

        /// <summary>
        /// Alternative: Remove field and insert image at its position
        /// </summary>
        public byte[] FillPdfImageFormFields(
            byte[] templateBytes,
            Dictionary<string, byte[]> imageFieldValues,
            bool flattenForm = true)
        {
            MemoryStream outputStream = null;

            try
            {
                var inputStream = new MemoryStream(templateBytes);
                outputStream = new MemoryStream();

                var reader = new PdfReader(inputStream);
                var writer = new PdfWriter(outputStream);
                var pdfDoc = new PdfDocument(reader, writer);

                var form = PdfAcroForm.GetAcroForm(pdfDoc, true);

                if (form == null)
                {
                    throw new InvalidOperationException("PDF template does not contain form fields");
                }

                var availableFields = form.GetAllFormFields();
                _logger.LogInformation($"Found {availableFields.Count} form fields");

                int filledCount = 0;

                foreach (var imageFieldEntry in imageFieldValues)
                {
                    string fieldName = imageFieldEntry.Key;
                    byte[] imageBytes = imageFieldEntry.Value;

                    if (imageBytes == null || imageBytes.Length == 0)
                    {
                        _logger.LogWarning($"Image bytes empty for '{fieldName}'");
                        continue;
                    }

                    if (availableFields.ContainsKey(fieldName))
                    {
                        try
                        {
                            var field = availableFields[fieldName];
                            var widgets = field.GetWidgets();

                            if (widgets == null || widgets.Count == 0)
                            {
                                _logger.LogWarning($"No widgets for '{fieldName}'");
                                continue;
                            }

                            var widget = widgets[0];
                            var page = widget.GetPage();
                            var rect = widget.GetRectangle().ToRectangle();

                            float x = rect.GetX();
                            float y = rect.GetY();
                            float width = rect.GetWidth();
                            float height = rect.GetHeight();

                            _logger.LogInformation($"Field '{fieldName}' at ({x}, {y}), size {width}x{height}");

                            // Create image
                            var imageData = ImageDataFactory.Create(imageBytes);
                            var image = new Image(imageData);

                            // Scale image to fit field
                            image.ScaleToFit(width, height);

                            // FIX: GetPageNumber is a method of PdfDocument, not PdfPage
                            int pageNumber = pdfDoc.GetPageNumber(page);
                            image.SetFixedPosition(pageNumber, x, y);

                            // Remove the field BEFORE adding image
                            form.RemoveField(fieldName);

                            // Add image to document
                            var document = new Document(pdfDoc);
                            document.Add(image);
                            // DON'T close document here - it will close pdfDoc too

                            filledCount++;
                            _logger.LogInformation($"✓ Inserted image at '{fieldName}' on page {pageNumber}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Failed for '{fieldName}': {ex.Message}");
                            _logger.LogError($"Stack trace: {ex.StackTrace}");
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"Field '{fieldName}' not found");
                    }
                }

                _logger.LogInformation($"Filled {filledCount}/{imageFieldValues.Count} images");

                if (flattenForm)
                {
                    form.FlattenFields();
                    _logger.LogInformation("Form flattened");
                }

                pdfDoc.Close();

                return outputStream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                throw;
            }
            finally
            {
                outputStream?.Dispose();
            }
        }

        /// <summary>
        /// List all form fields in a PDF for debugging
        /// </summary>
        public List<string> GetFormFieldNames(byte[] pdfBytes)
        {
            try
            {
                using var inputStream = new MemoryStream(pdfBytes);
                using var reader = new PdfReader(inputStream);
                using var pdfDoc = new PdfDocument(reader);

                var form = PdfAcroForm.GetAcroForm(pdfDoc, false);

                if (form == null)
                {
                    return new List<string>();
                }

                return form.GetAllFormFields().Keys.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting form field names: {ex.Message}");
                return new List<string>();
            }
        }
    }
}