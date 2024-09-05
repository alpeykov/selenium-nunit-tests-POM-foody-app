using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Foody
{
    public class BasePage
    {
        protected IWebDriver Driver { get; private set; }
        protected WebDriverWait Wait { get; private set; }
        public Actions Actions { get; private set; }

        // Static variables
        private static readonly string BaseUrl = "http://softuni-qa-loadbalancer-2137572849.eu-north-1.elb.amazonaws.com:85/";
        private static string? lastCreatedTitle;
        private static string? lastCreatedDescription;
        private static string? lastCreatedTitleEdited;
        private static int? numberOfDisplayedCards;
        private static string? userName;

        // Accessor for BaseUrl
        public static string GetBaseUrl()
        {
            return BaseUrl;
        }

        // Initialize the Page
        public BasePage(IWebDriver driver, TimeSpan waitTime)
        {
            Driver = driver;
            Wait = new WebDriverWait(driver, waitTime);
            Actions = new Actions(driver);
        }

        // Find SINGLE element
        public static class Find
        {
            private static IWebDriver driver;
            private static WebDriverWait wait;

            public static void Initialize(IWebDriver webDriver, TimeSpan timeout)
            {
                driver = webDriver;
                wait = new WebDriverWait(driver, timeout);
            }

            private static IWebElement WaitForElement(Func<By> bySelector)
            {
                return wait.Until(ExpectedConditions.ElementIsVisible(bySelector()));
            }

            public static Func<string, IWebElement> Id => locator => WaitForElement(() => By.Id(locator));
            public static Func<string, IWebElement> Name => locator => WaitForElement(() => By.Name(locator));
            public static Func<string, IWebElement> Css => locator => WaitForElement(() => By.CssSelector(locator));
            public static Func<string, IWebElement> XPath => locator => WaitForElement(() => By.XPath(locator));
            public static Func<string, IWebElement> ClassName => locator => WaitForElement(() => By.ClassName(locator));
            public static Func<string, IWebElement> TagName => locator => WaitForElement(() => By.TagName(locator));
            public static Func<string, IWebElement> LinkText => locator => WaitForElement(() => By.LinkText(locator));
            public static Func<string, IWebElement> PartialLinkText => locator => WaitForElement(() => By.PartialLinkText(locator));
        }

        // Find MULTIPLE elements
        public static class Finds
        {
            private static IWebDriver driver;
            private static WebDriverWait wait;

            public static void Initialize(IWebDriver webDriver, TimeSpan timeout)
            {
                driver = webDriver;
                wait = new WebDriverWait(driver, timeout);
            }

            private static IReadOnlyCollection<IWebElement> WaitForElements(Func<By> bySelector)
            {
                return wait.Until(drv => drv.FindElements(bySelector()));
            }

            public static Func<string, IReadOnlyCollection<IWebElement>> Id => locator => WaitForElements(() => By.Id(locator));
            public static Func<string, IReadOnlyCollection<IWebElement>> Name => locator => WaitForElements(() => By.Name(locator));
            public static Func<string, IReadOnlyCollection<IWebElement>> Css => locator => WaitForElements(() => By.CssSelector(locator));
            public static Func<string, IReadOnlyCollection<IWebElement>> XPath => locator => WaitForElements(() => By.XPath(locator));
            public static Func<string, IReadOnlyCollection<IWebElement>> ClassName => locator => WaitForElements(() => By.ClassName(locator));
            public static Func<string, IReadOnlyCollection<IWebElement>> TagName => locator => WaitForElements(() => By.TagName(locator));
            public static Func<string, IReadOnlyCollection<IWebElement>> LinkText => locator => WaitForElements(() => By.LinkText(locator));
            public static Func<string, IReadOnlyCollection<IWebElement>> PartialLinkText => locator => WaitForElements(() => By.PartialLinkText(locator));
        }

        // Utility methods
        public string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        // Create new item
        public void CreateItem(string foodName, string foodDescription)
        {
            var addFoodLink = Find.XPath("//a[@class='nav-link'][contains(.,'Add Food')]");
            addFoodLink.Click();

            var foodNameInput = Find.Css("input#name");
            foodNameInput.Clear();
            foodNameInput.SendKeys(foodName);

            var foodDescriptionInput = Find.Css("input#description");
            foodDescriptionInput.Clear();
            foodDescriptionInput.SendKeys(foodDescription);

            Find.Css("input#url").SendKeys("https://cdn.britannica.com/71/182071-050-4081A3AB/Poutine.jpg");

            var addBtn = Find.Css("[type='submit']");
            addBtn.Click();
        }

        // Get last item in the list
        public IWebElement GetLastCard()
        {
            var cards = Wait.Until(driver => driver.FindElements(By.CssSelector(".row.gx-5.align-items-center")));
            Assert.That(cards.Count, Is.GreaterThan(0));
            return cards.Last();
        }

        // Search method
        public void Search(string searchCriteria)
        {
            var searchInput = Find.XPath("//input[contains(@type,'search')]");
            searchInput.Clear();
            searchInput.SendKeys(searchCriteria);

            var searchBtn = Find.Css(".btn.btn-primary.col-2.mt-5.rounded-pill > svg[role='img']");
            searchBtn.Click();
        }

        // Count displayed items
        public void CountDisplayedElements()
        {
            var elements = Driver.FindElements(By.CssSelector(".row.gx-5.align-items-center"));
            numberOfDisplayedCards = elements.Count();
        }

        // Login
        public void Login(string userName)
        {
            Find.XPath("//a[@class='nav-link'][contains(.,'Log In')]").Click();

            var userNameInput = Find.Css("input#username");
            userNameInput.Clear();
            userNameInput.SendKeys(userName);

            var passwordInput = Find.Css("input#password");
            passwordInput.Clear();
            passwordInput.SendKeys("123456");

            var loginBtn = Find.Css("[type='submit']");
            loginBtn.Click();
        }

        // Register new user
        public void Registration()
        {
            userName = $"alp{GenerateRandomString(3)}";
            Find.XPath("//a[@class='nav-link'][contains(.,'Sign Up')]").Click();

            var usernameInput = Find.XPath("//input[contains(@id,'username')]");
            usernameInput.Clear();
            usernameInput.SendKeys(userName);

            var emailInput = Find.XPath("//input[contains(@type,'email')]");
            emailInput.Clear();
            emailInput.SendKeys($"{userName}@yahoo.com");

            var firstNameInput = Find.XPath("//input[contains(@id,'firstName')]");
            firstNameInput.Clear();
            firstNameInput.SendKeys("Al");

            var middleNameInput = Find.XPath("//input[contains(@id,'midName')]");
            middleNameInput.Clear();
            middleNameInput.SendKeys("Pl");

            var lastNameInput = Find.XPath("//input[contains(@id,'lastName')]");
            lastNameInput.Clear();
            lastNameInput.SendKeys("Pe");

            var password = Find.XPath("//input[contains(@id,'password')]");
            password.Clear();
            password.SendKeys("123456");

            var rePassword = Find.XPath("//input[contains(@id,'rePassword')]");
            rePassword.Clear();
            rePassword.SendKeys("123456");

            var singupBtn = Find.XPath("//button[contains(.,'Sign up')]");
            Actions.MoveToElement(singupBtn).Click().Perform();
            //singupBtn.Click();

            Console.WriteLine($"Username:{userName}");
        }

        // Static properties for test methods
        public static string? LastCreatedTitle
        {
            get => lastCreatedTitle;
            set => lastCreatedTitle = value;
        }

        public static string? LastCreatedDescription
        {
            get => lastCreatedDescription;
            set => lastCreatedDescription = value;
        }

        public static string? LastCreatedTitleEdited
        {
            get => lastCreatedTitleEdited;
            set => lastCreatedTitleEdited = value;
        }

        public static int? NumberOfDisplayedCards
        {
            get => numberOfDisplayedCards;
            set => numberOfDisplayedCards = value;
        }

        public static string? UserName
        {
            get => userName;
            set => userName = value;
        }
    }
}
