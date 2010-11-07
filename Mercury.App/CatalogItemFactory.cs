using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime;
using System.Text;

namespace Mercury.App
{
    public static class CatalogItemFactory
    {
        public static IList<CatalogItem> GetStartMenuItems()
        {
            String startMenu = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);

            List<CatalogItem> itemList = new List<CatalogItem>();

            BuildList(startMenu, itemList);

            return itemList.AsReadOnly();
        }

        private static void BuildList(string startMenu, List<CatalogItem> itemList)
        {
            foreach (String file in Directory.GetFiles(startMenu))
            {
                itemList.Add(new CatalogItem(Path.Combine(startMenu, file)));
            }

            foreach (String dir in Directory.GetDirectories(startMenu))
            {
                if (dir != ".." && dir != ".")
                {
                    BuildList(Path.Combine(startMenu, dir), itemList);
                }
            }
        }
    }
}
