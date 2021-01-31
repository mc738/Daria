/*using System;
using System.Collections.Generic;
using FDOM.Core.Common;
using Microsoft.AspNetCore.Components;

namespace FDOM.Blazor
{
    public class RenderFactory
    {
        public RenderFragment RenderArticle(DOM.Document article)
        {
            RenderFragment content() => builder =>
            {
                builder.OpenElement(1, "div");
                builder.AddAttribute(2, "class", "all-or-noting");

                // If the paragraph is split into spans, add eat dynamically, or add node value if it doesn't.

                int step = 3;

                foreach (var child in article.Sections)
                {
                    builder.AddContent(step, RenderNode(child));

                    step++;
                }

                //builder.AddAttribute(1, "OpenProject",EventCallback.Factory.Create<Guid>(this, OpenProjectTab));
                builder.CloseElement();
            };
            
            return content();
        }

        private RenderFragment RenderNode(DOM.Section section)
        {

            var children = new List<RenderFragment>();
            
            foreach (var item in section.Content)
            {
                // Not the prettiest, but nicer than a switch on magic ints.
                if (item.IsHeader)
                    children.Add(RenderHeader((DOM.HeaderBlock)item));
                else if (item.IsImage)
                    children.Add(RenderImage(item));
                else if (item.IsList)
                    children.Add(RenderList(item));
                else if (item.IsParagraph)
                    children.Add(RenderParagraph(item));
            }

            
            
            return null;
        }
        
        private RenderFragment RenderSection(INode node)
        {
            RenderFragment content() => builder =>
            {
                builder.OpenElement(1, "div");
                builder.AddAttribute(2, "class", node.GetMetaData<string>("class"));

                // If the paragraph is split into spans, add eat dynamically, or add node value if it doesn't.
                if (node.HasChildren)
                {
                    int step = 3;

                    foreach (var child in node.Children)
                    {



                        builder.AddContent(step, RenderNode(child));
                        step++;

                    }
                }
                //builder.AddAttribute(1, "OpenProject",EventCallback.Factory.Create<Guid>(this, OpenProjectTab));
                builder.CloseElement();
            };


            return content();
        }

        private RenderFragment RenderHeader(INode node)
        {
            RenderFragment content() => builder =>
            {
                builder.OpenElement(1, "h1");
                builder.AddAttribute(2, "class", node.GetMetaData<string>("class"));

                // If the paragraph is split into spans, add eat dynamically, or add node value if it doesn't.
                if (node.HasChildren)
                {
                    int step = 3;

                    foreach (var child in node.Children)
                    {
                        builder.AddContent(step, RenderNode(child));
                        step++;

                    }
                }
                else
                {
                    builder.AddContent(3, node.Value);
                }
                //builder.AddAttribute(1, "OpenProject",EventCallback.Factory.Create<Guid>(this, OpenProjectTab));
                builder.CloseElement();
            };


            return content();
        }

        private RenderFragment RenderParagraph(INode node)
        {
            RenderFragment content() => builder =>
            {
                builder.OpenElement(1, "p");
                builder.AddAttribute(2, "class", node.GetMetaData<string>("class"));

                // If the paragraph is split into spans, add eat dynamically, or add node value if it doesn't.
                if (node.HasChildren)
                {
                    int step = 3;

                    foreach (var child in node.Children)
                    {
                        builder.AddContent(step, RenderNode(child));
                        step++;

                    }
                }
                else
                {
                    builder.AddContent(3, node.Value);

                }

                //builder.AddAttribute(1, "OpenProject",EventCallback.Factory.Create<Guid>(this, OpenProjectTab));
                builder.CloseElement();
            };


            return content();
        }

        private RenderFragment RenderSpan(INode node)
        {
            RenderFragment content() => builder =>
            {
                builder.OpenElement(1, "span");
                builder.AddAttribute(2, "class", node.GetMetaData<string>("class"));
                builder.AddContent(3, node.Value);
                //builder.AddAttribute(1, "OpenProject",EventCallback.Factory.Create<Guid>(this, OpenProjectTab));
                builder.CloseElement();
            };

            return content();
        }

        private RenderFragment RenderImage(INode node)
        {
            RenderFragment content() => builder =>
            {
                builder.OpenElement(1, "div");
                builder.AddAttribute(2, "class", "image-holder");
                builder.OpenElement(3, "div");
                builder.OpenElement(4, "img");
                builder.AddAttribute(5, "class", "article-image");
                builder.AddAttribute(6, "src", node.GetMetaData<string>("src"));
                builder.CloseElement();
                builder.OpenElement(7, "p");
                builder.AddAttribute(8, "class", "image-description");
                builder.AddContent(9, node.Value);
                builder.CloseElement();
                builder.CloseElement();
                builder.CloseElement();
            };

            return content();
        }
        
        private RenderFragment RenderList(INode node)
        {
            RenderFragment content() => builder =>
            {
                builder.OpenElement(1, "ol");
                builder.AddAttribute(2, "class", node.GetMetaData<string>("class"));

                // If the paragraph is split into spans, add eat dynamically, or add node value if it doesn't.

                int step = 3;

                foreach (var child in node.Children)
                {
                    builder.AddContent(step, RenderNode(child));
                    step++;

                }

                //builder.AddAttribute(1, "OpenProject",EventCallback.Factory.Create<Guid>(this, OpenProjectTab));
                builder.CloseElement();
            };


            return content();
        }

        private RenderFragment RenderListItem(INode node)
        {
            RenderFragment content() => builder =>
            {
                builder.OpenElement(1, "li");
                builder.AddAttribute(2, "class", node.GetMetaData<string>("class"));


                int step = 3;

                foreach (var child in node.Children)
                {
                    builder.AddContent(step, RenderNode(child));
                    step++;

                }


                //builder.AddAttribute(1, "OpenProject",EventCallback.Factory.Create<Guid>(this, OpenProjectTab));
                builder.CloseElement();
            };


            return content();
        }
    }
}*/