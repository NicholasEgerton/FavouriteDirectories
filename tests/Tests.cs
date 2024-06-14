using FavouriteDirectories;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Tests
{
    [TestClass]
    public class UnitTests
    {
        [TestMethod]
        public void TestAddDirectorySeperator()
        {
            //Function:
            //internal static string AddDirectorySeperator(string s)

            //Arrange
            string[] inputs = { "C:", @"F:\Users\(user)\Downloads",
            @"X:/", @"t:\/,", @"long, completelynot.\d/valid\..path" };

            //Note: "ex" means Expected
            string[] exOutputs = { @"C:\", @"F:\Users\(user)\Downloads\",
                @"X:/", @"", @""};


            if(inputs.Length != exOutputs.Length)
            {
                throw new Exception("InputsLength != Expected Output length");
            }


            for(int i = 0; i < inputs.Length; i++)
            {
                //Act
                string output = Fav.AddDirectorySeperator(inputs[i]);

                //Assert
                Assert.AreEqual(output, exOutputs[i], "Unexpected output");
            }
        }
    }
    [TestClass]
    public class IntegrationTests
    {
        [TestMethod]
        public void RunIntegrationTests()
        {
            //Reset the favourites file to clear previous tests
            Fav fav = new Fav(new string[] {"reset"});

            //EXAMPLE INPUTS
            const int MIN_TESTS = 5; //Don't go under this

            string[] inputDirectories = {@"C:\", "C:/"
            , @"C:\Users", @"T:\", @"fsdf//.fefe"};

            //EXAMPLE OUTPUTS
            //Note: "ex" means Expected
            const string exOut = "C:\\\r\nC:/\r\nC:\\Users\\\r\n";

            int x = 0;
            //Add some directories
            while (x < MIN_TESTS)
            {
                fav = new Fav(new string[] {"add", inputDirectories[x] });
                x++;
            }

            int y = 0;
            //Remove and move to some directories
            while (y < MIN_TESTS)
            {
                string[] args;
                //Even
                if (y % 2 == 0)
                {

                    args = new string[]{ "cd", inputDirectories[y] };
                }

                //Odd
                else
                { 
                    args = new string[]{ "remove", inputDirectories[y] };
                }

                fav = new Fav(args);
                y++;
            }

            int z = 0;
            while(z < MIN_TESTS)
            {
                string[] args;
                //Even
                if (z % 2 == 0)
                {
                    args = new string[] { "help" };
                }
                //Odd
                else
                {
                    args = new string[] { "show" };
                }
                fav = new Fav(args);
                z++;
            }

            //Path to favourites.txt
            //Where the favourites will be stored
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "favourites.txt");

            string favourites;

            //Load favourites
            using (StreamReader streamReader = new StreamReader(path, Encoding.UTF8))
            {
                favourites = streamReader.ReadToEnd();
            }

            //Assert

            Assert.AreEqual(exOut, favourites, "Unexpected output");
        }
    }
}