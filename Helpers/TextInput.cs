using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace cce106_palit.Helpers
{
    [HtmlTargetElement("text-input")]
    public class TextInput : TagHelper
    {
        public required ModelExpression AspFor { get; set; }
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Type { get; set; } = "text";
        public string Placeholder { get; set; } = "";
        public bool Disabled { get; set; } = false;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "input";
            output.TagMode = TagMode.SelfClosing;

            var name = string.IsNullOrEmpty(Name) ? AspFor.Name : Name;
            var id = string.IsNullOrEmpty(Id) ? AspFor.Name : Id;
            var value = AspFor.Model?.ToString() ?? "";

            output.Attributes.SetAttribute("type", Type);
            output.Attributes.SetAttribute("id", id);
            output.Attributes.SetAttribute("name", name);
            output.Attributes.SetAttribute("value", value);
            output.Attributes.SetAttribute("placeholder", Placeholder);

            if (Disabled)
            {
                output.Attributes.SetAttribute("disabled", "disabled");
            }

            output.Attributes.SetAttribute("class",
                "border-gray-800 focus:border-blue-500 focus:ring-blue-500 rounded-md shadow-sm block w-full px-4 py-2 text-gray-800 bg-white dark:bg-gray-700 dark:text-gray-300 dark:border-gray-600 focus:ring-opacity-40 dark:focus:border-blue-300 focus:outline-none focus:ring focus:ring-blue-300"
            );
        }
    }
}
