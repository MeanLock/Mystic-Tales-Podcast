using Microsoft.EntityFrameworkCore;


namespace SurveyTalkService.API.GraphQL.Features.Template.Types
{
    public class Template{
        public string Name { get; set; }
        public string Description { get; set; }
    }
    public class TemplateType : ObjectType<Template>
    {
        protected override void Configure(IObjectTypeDescriptor<Template> descriptor)
        {
            descriptor.Name("TemplateDayNe");   // descriptor.Name("TemplateType") trong ObjectTypeExtension<T> chỉ định tên type mục tiêu để extend chứ không có tác dụng đặt tên cho type mới
            descriptor.Field(t => t.Name).Name("Name").Type<StringType>();
            descriptor.Field(t => t.Description).Name("Description").Type<StringType>();
            
        }
    }


}
