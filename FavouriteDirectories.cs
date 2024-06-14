using System;
using System.IO;
using System.Text;
using System.Diagnostics;


namespace FavouriteDirectories
{
    public class ProgramEntry
    {
        public static void Main(string[] args)
        {
            Fav fav = new Fav(args);
        }
    }

    public class Fav
    {
        public Fav(string[] args)
        {
            //Path to favourites.txt
            //Where the favourites will be stored
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "favourites.txt");

            //Check if favourites file exists
            if (!File.Exists(path))
            {
                Console.WriteLine("No saved favourites file detected! Making one now... \nType 'fav help to find commands.'");
                File.Create(path).Close();
            }

            string favourites;

            //Load favourites
            using (StreamReader streamReader = new StreamReader(path, Encoding.UTF8))
            {
                favourites = streamReader.ReadToEnd();
            }
            
            ConsiderArgs(args, path, favourites);
        }

        public void ConsiderArgs(string[] args, string path, string favourites)
        {
            //If the user has just typed "fav" then go to first favourite
            if (args.Length < 1)
            {
                if (favourites.Length == 0 || favourites == null)
                {
                    Console.WriteLine("No favourites added yet.\nType 'fav add (directory name)' to add one!");
                    return;
                }

                else
                {
                    MoveDirectory(favourites, "1");
                    return;
                }
            }

            //Save args a string instead of string array
            string fullArgs = string.Join(" ", args);
            //Save favourites as an arr instead of a string
            string[] favouritesArr = favourites.Split("\n");

            //Consider arguments
            if (args.Length == 1)
            {
                switch (args[0])
                {
                    case "show":
                        ShowDirectories(favouritesArr);
                        break;

                    case "help":
                        SendHelpMessage();
                        break;


                    case "reset":
                        ResetDirectories(path);
                        break;

                    default:
                        Console.WriteLine("No valid command: '" + fullArgs + "'\nType 'fav help' to find commands.");
                        break;
                }
            }

            else if (args.Length == 2)
            {
                switch (args[0])
                {
                    //Adding a directory
                    case "add":
                        AddDirectory(args, fullArgs, path, favourites);
                        break;

                    //Moving to a directory
                    case "cd":
                        MoveDirectory(favourites, args[1]);
                        break;
                    //Removing a directory
                    case "remove":
                        RemoveDirectory(favouritesArr, args[1], path);
                        break;

                    default:
                        Console.WriteLine("No valid command: '" + fullArgs + "'\nType 'fav help' to find commands.");
                        break;
                }
            }

            else
            {
                Console.WriteLine("Command is not valid or used correctly in: '" + fullArgs + "' \nType 'fav help' to find commands.");
            }
        }

        public void ShowDirectories(string[] favouritesArr)
        {

            if (favouritesArr.Length == 0)
            {
                Console.WriteLine("No saved favourites detected.\nType 'fav add (directory name)'");
                return;
            }

            else if (favouritesArr[0] == "")
            {
                Console.WriteLine("No saved favourites detected.\nType 'fav add (directory name)'");
                return;
            }

            //Loop through the array and display the saved directories
            int count = 1;
            for (int i = 0; i < favouritesArr.Length; i++)
            {
                if (favouritesArr[i] != "" && favouritesArr[i] != "\r" && favouritesArr[i] != "\n")
                {
                    Console.WriteLine(count.ToString() + " = " + favouritesArr[i]);
                    count++;
                }
            }
        }

        public void SendHelpMessage()
        {
            const string helpMsg = "Here are useful commands you can use fav:\n" +
                            "'fav add (directory name)' = Adds a new directory (directory name) to the list of favourites\n" +
                            "'fav cd (index of favourite)' = Changes cmd directory to the favourite of (index), starting from 1\n" +
                            "'fav show' = Shows all the saved directories.\n" +
                            "'fav remove (index of favourite)' = Removes the saved favourite at (index)." +
                            "'fav reset' = Removes all saved favourites";
            Console.WriteLine(helpMsg);
        }


        public void ResetDirectories(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine("Cannot remove favourites as the file does not exist!");
                return;
            }

            //Sets the file to empty
            File.WriteAllText(path, string.Empty);
        }

        public void AddDirectory(string[] args, string fullArgs, string path, string favourites)
        {
            //The directory to add
            string dir = AddDirectorySeperator(args[1].TrimEnd());

            //Check if directory exists
            if (!Directory.Exists(dir))
            {
                Console.WriteLine("No valid directory: " + args[1]);
                return;
            }

            //Split into string[]
            string[] favouritesArr = favourites.Split("\n");

            if (dir != "")
            {
                //Check it is not already in file
                if (!favouritesArr.Contains(dir + "\r", StringComparer.OrdinalIgnoreCase)) 
                {
                    //Write the directory to text file
                    //Note: The second argument is "true" to make it so
                    //Append and not remove other saved directories
                    using (StreamWriter streamWriter = new StreamWriter(path, true))
                    {
                        streamWriter.WriteLine(dir);
                    }
                }

                else
                {
                    Console.WriteLine("Directory already saved!");
                    return;
                }
            }

            else
            {
                Console.WriteLine("Failed to get Directory name!");
                return;
            }
        }

        public void MoveDirectory(string favourites, string index)
        {
            if (favourites.Length == 0 || favourites == null)
            {
                Console.WriteLine("No saved favourites detected.\nType 'fav add (directory name)'");
                return;
            }

            string[] favouritesArr = favourites.Split("\n");

            //Split will usually leave an empty string at end,
            //Removing it here.
            if (favouritesArr.Last() == "")
            {
                favouritesArr = favouritesArr.Take(favouritesArr.Length - 1).ToArray();
            }

            //Note: the index is being inputed in a 1, 2, 3, 4... form
            //Not a 0,1,2,3...
            int i;

            if (int.TryParse(index, out i))
            {
                //Bring i back to 0,1,2,3 form
                i -= 1;

                //Check if it is a valid index
                if (favouritesArr.Length > i && i > -1)
                {
                    if (!Directory.Exists(favouritesArr[i].Replace("\r", string.Empty)))
                    {
                        Console.WriteLine("That directory is not valid.\nType 'fav show' to view it");
                        return;
                    }

                    //Create and setup a new cmd process
                    Process p = new Process();

                    //Use shell execute ensures it launches in a new window
                    p.StartInfo.UseShellExecute = true;
                    p.StartInfo.FileName = "cmd.exe";

                    //Important to remove \r from directory otherwise
                    //It will be invalid
                    p.StartInfo.WorkingDirectory = favouritesArr[i].Replace("\r", string.Empty);

                    p.Start();
                }

                else
                {
                    Console.WriteLine("That index of favourites does not exist.");
                }
            }

            else
            {
                Console.WriteLine("That index of favourites does not exist.");
            }
        }

        public void RemoveDirectory(string[] favouritesArr, string index, string path)
        {
            int i;
            //Get index
            if (!int.TryParse(index, out i))
            {
                Console.WriteLine("That index of favourites does not exist.");
                return;
            }


            //Bring index back to 0,1,2,3
            i--;

            if (favouritesArr.Length <= i)
            {
                Console.WriteLine("That index of favourites does not exist.");
                return;
            }

            else if (favouritesArr[i] == "")
            {
                Console.WriteLine("That index of favourites does not exist.");
                return;
            }

            //Convert to list
            List<string> l = favouritesArr.ToList();

            //Delete the index
            l.RemoveAt(i);

            //Go back to array
            favouritesArr = l.ToArray();

            //Output the new favourites
            //Note: Second argument is "false" to not append and
            //Instead reset the file
            using (StreamWriter streamWriter = new StreamWriter(path, false))
            {
                string s = string.Join("\n", favouritesArr);
                streamWriter.Write(s);
            }
        }

        public static string AddDirectorySeperator(string s)
        {
            if(s == null || s == "")
            {
                Console.WriteLine("Failed to add DirectorySeperator");
                return "";
            }

            if(s.Contains(Path.DirectorySeparatorChar))
            {
                if(s.Contains(Path.AltDirectorySeparatorChar))
                {
                    Console.WriteLine("Cannot use both '\' and '/' in path!");
                    return "";
                }

                else if(!s.EndsWith(Path.DirectorySeparatorChar))
                {
                    s += Path.DirectorySeparatorChar;
                }
            }

            else if(s.Contains(Path.AltDirectorySeparatorChar))
            {
                if (!s.EndsWith(Path.AltDirectorySeparatorChar))
                {
                    s += Path.AltDirectorySeparatorChar;
                }
            }

            else
            {
                s += Path.DirectorySeparatorChar;
            }

            return s;
        }
    }
}
