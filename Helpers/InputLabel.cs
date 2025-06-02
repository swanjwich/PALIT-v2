using Microsoft.AspNetCore.Razor.TagHelpers;

namespace cce106_palit.Helpers
{
    [HtmlTargetElement("input-label")]
    public class InputLabel : TagHelper
    {
        public string For { get; set; } = "";
        public string Value { get; set; } = "";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "label";
            output.TagMode = TagMode.StartTagAndEndTag;

            output.Attributes.SetAttribute("class",
               "mb-2 text-sm font-medium dark:text-gray-300"
           );

            if (!string.IsNullOrEmpty(For))
            {
                output.Attributes.SetAttribute("for", For);
            }

            output.Content.SetContent(Value);
        }
    }
}
