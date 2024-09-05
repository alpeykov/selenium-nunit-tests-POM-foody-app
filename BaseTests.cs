using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;

namespace Foody
{
    [TestFixture]
    public class BaseTests
    {
        private IWebDriver driver;
        private BasePage basePage;

        [OneTimeSetUp]
        public void Setup()
        {
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddUserProfilePreference("profile.password_manager_enabled", false);
            chromeOptions.AddArgument("--disable-search-engine-choice-screen");

            driver = new ChromeDriver(chromeOptions);
            driver.Manage().Window.Maximize();
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            driver.Navigate().GoToUrl(BasePage.GetBaseUrl());

            basePage = new BasePage(driver, TimeSpan.FromSeconds(10));
            BasePage.Find.Initialize(driver, TimeSpan.FromSeconds(10));
            BasePage.Finds.Initialize(driver, TimeSpan.FromSeconds(10));

            basePage.Registration();
            // basePage.Login("alp", "123456");
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            driver.Close();
            driver.Quit();
            driver.Dispose();
        }

        // Test #1 - Add item with INVALID data
        [Test, Order(1)]
        public void AddItemInvalidData()
        {
            driver.Navigate().GoToUrl(BasePage.GetBaseUrl());
            basePage.CreateItem(" ", " ");

            string expectedUrl = $"{BasePage.GetBaseUrl()}Food/Add";
            Assert.That(driver.Url, Is.EqualTo(expectedUrl), "The URL after creation did not match the expected URL.");

            var mainErrorMsg = BasePage.Find.Css(".text-danger.validation-summary-errors > ul > li");
            var titleErrorMsg = BasePage.Find.Css("[data-valmsg-for='Name']");
            var descriptionErrorMsg = BasePage.Find.Css("[data-valmsg-for='Description']");

            Assert.That(mainErrorMsg.Text, Is.EqualTo("Unable to add this food revue!"));
            Assert.That(titleErrorMsg.Text, Is.EqualTo("The Name field is required."));
            Assert.That(descriptionErrorMsg.Text, Is.EqualTo("The Description field is required."));
        }

        // Test #2 - Add item with VALID data
        [Test, Order(2)]
        public void AddItemValidData()
        {
            driver.Navigate().GoToUrl(BasePage.GetBaseUrl());
            BasePage.LastCreatedTitle = $"Food {basePage.GenerateRandomString(5)}";
            BasePage.LastCreatedDescription = $"Description {basePage.GenerateRandomString(10)}";

            BasePage.Find.XPath("//a[@class='nav-link'][contains(.,'Add Food')]").Click();

            basePage.CreateItem(BasePage.LastCreatedTitle, BasePage.LastCreatedDescription);
            Console.WriteLine($"Last created Title:{BasePage.LastCreatedTitle}");
            Console.WriteLine($"Last created Description:{BasePage.LastCreatedDescription}");

            string expectedUrl = $"{BasePage.GetBaseUrl()}";
            Assert.That(driver.Url, Is.EqualTo(expectedUrl), "The URL after creation did not match the expected URL.");

            var lastCard = basePage.GetLastCard();
            var lastCreatedTitleDisplayed = lastCard.FindElement(By.CssSelector("div.p-5>h2"));
            Console.WriteLine($"Title text of the last item in the list: {lastCreatedTitleDisplayed.Text}");
            Assert.That(lastCreatedTitleDisplayed.Text, Is.EqualTo(BasePage.LastCreatedTitle));

            basePage.CountDisplayedElements();
            Console.WriteLine($"The number of displayed cards is: {BasePage.NumberOfDisplayedCards}");
        }

        // Test #3 - Edit the last created item
        [Test, Order(3)]
        public void EditLastAddedItem()
        {
            driver.Navigate().GoToUrl(BasePage.GetBaseUrl());

            BasePage.LastCreatedTitleEdited = $"Edited Title {basePage.GenerateRandomString(3)}";
            basePage.CountDisplayedElements();
            Console.WriteLine($"The number of displayed cards BEFORE EDIT is: {BasePage.NumberOfDisplayedCards}");

            var lastCard = basePage.GetLastCard();
            var lastEditBtn = lastCard.FindElement(By.CssSelector("a[href*='/Food/Edit']"));
            basePage.Actions.MoveToElement(lastEditBtn).Click().Perform();

            basePage.CreateItem(BasePage.LastCreatedTitleEdited, BasePage.LastCreatedDescription);

            lastCard = basePage.GetLastCard();
            var lastCreatedTitleDisplayed = lastCard.FindElement(By.CssSelector("div.p-5>h2"));
            Console.WriteLine($"Title text of the last item in the list: {lastCreatedTitleDisplayed.Text}");
            basePage.CountDisplayedElements();

            var cards = driver.FindElements(By.CssSelector(".row.gx-5.align-items-center"));
            bool titleFound = cards.Any(card =>
            {
                var titleElement = card.FindElement(By.CssSelector("div.p-5 > h2"));
                return titleElement.Text.Equals(BasePage.LastCreatedTitle, StringComparison.OrdinalIgnoreCase);
            });

            Assert.That(lastCreatedTitleDisplayed.Text, Is.EqualTo(BasePage.LastCreatedTitleEdited));
            Assert.IsTrue(titleFound, "The card with the last created title was FOUND.");
            Console.WriteLine($"The number of displayed cards AFTER EDIT is: {BasePage.NumberOfDisplayedCards}");
            Console.WriteLine($"!!BUG DETECTED!!! Edit functionality creates new item, instead of editing the last created one.");
        }

        // Test #4 - Search for last created item
        [Test, Order(4)]
        public void SearchForLastCreatedItem()
        {
            driver.Navigate().GoToUrl(BasePage.GetBaseUrl());

            basePage.Search(BasePage.LastCreatedTitle);
            basePage.CountDisplayedElements();
            Console.WriteLine($"The number of displayed cards is: {BasePage.NumberOfDisplayedCards}");
            Assert.That(BasePage.NumberOfDisplayedCards, Is.EqualTo(1));

            var lastCreatedTitleDisplayed = driver.FindElement(By.CssSelector("div.p-5>h2"));
            Console.WriteLine($"Title text of the last item in the SEARCH list: {lastCreatedTitleDisplayed.Text}");
            Assert.That(lastCreatedTitleDisplayed.Text, Is.EqualTo(BasePage.LastCreatedTitle));
        }

        // Test #5 - Delete last created item
        [Test, Order(5)]
        public void DeleteLastItem()
        {
            driver.Navigate().GoToUrl(BasePage.GetBaseUrl());

            basePage.CountDisplayedElements();
            Console.WriteLine($"The number of displayed cards BEFORE delete is: {BasePage.NumberOfDisplayedCards}");
            var DisplayedCardsBefore = BasePage.NumberOfDisplayedCards;

            var lastCard = basePage.GetLastCard();
            var lastEditBtn = lastCard.FindElement(By.CssSelector("a[href*='/Food/Delete']"));
            basePage.Actions.MoveToElement(lastEditBtn).Click().Perform();

            driver.Navigate().GoToUrl(BasePage.GetBaseUrl());
            basePage.CountDisplayedElements();
            Console.WriteLine($"The number of displayed cards AFTER delete is: {BasePage.NumberOfDisplayedCards}");
            var DisplayedCardsAfter = BasePage.NumberOfDisplayedCards;

            Assert.That(BasePage.NumberOfDisplayedCards, Is.EqualTo(DisplayedCardsBefore - 1));
        }

        // Test #6 - Perform search for last created item
        [Test, Order(6)]
        public void SearchForDeletedItem()
        {
            driver.Navigate().GoToUrl(BasePage.GetBaseUrl());
            basePage.Search(BasePage.LastCreatedTitleEdited);

            var searchMsg = BasePage.Find.XPath("//h2[@class='display-4']");
            Assert.That(searchMsg.Text, Is.EqualTo("There are no foods :("));

            var addBtn = BasePage.Find.XPath("//a[@class='btn btn-primary btn-xl rounded-pill mt-5'][contains(.,'Add food')]");
            Assert.IsTrue(addBtn.Displayed, "The 'Add food' button is not visible.");
        }

        // Test #7 - Logout
        [Test, Order(7)]
        public void Logout()
        {
            driver.Navigate().GoToUrl(BasePage.GetBaseUrl());
            BasePage.Find.XPath("//a[@href='/User/Logout']").Click();

            var loginBtn = BasePage.Find.XPath("//a[@href='/User/Login']");
            Assert.IsTrue(loginBtn.Displayed, "The 'Login' button is not visible.");

            var singupBtn = BasePage.Find.XPath("//a[@href='/User/Register']");
            Assert.IsTrue(singupBtn.Displayed, "The 'Sign up' button is not visible.");
        }

        // Test #8 - Perform Login
        [Test, Order(8)]
        public void PerformLogin()
        {
            driver.Navigate().GoToUrl(BasePage.GetBaseUrl());
            basePage.Login(BasePage.UserName);

            var addFoodBtn = BasePage.Find.XPath("//a[@href='/Food/Add']");
            Assert.IsTrue(addFoodBtn.Displayed, "The 'Add food' button is not visible.");

            var logoutBtn = BasePage.Find.XPath("//a[@href='/User/Logout']");
            Assert.IsTrue(logoutBtn.Displayed, "The 'Logout' button is not visible.");

            Console.WriteLine($"Last created user: {BasePage.UserName}");
        }
    }
}
