using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Globalization;
using System.Linq;

namespace SauceDemoAutomation.Tests
{
    [TestFixture]
    public class SauceDemoTests : IDisposable
    {
        private IWebDriver driver;
        private WebDriverWait wait;
        private const string BaseUrl = "https://www.saucedemo.com";

        // Reusable locators for page elements
        private static readonly By CartLink = By.ClassName("shopping_cart_link");
        private static readonly By CheckoutButton = By.Id("checkout");
        private static readonly By FinishButton = By.Id("finish");
        private static readonly By TitleHeader = By.ClassName("title");
        private static readonly By CompleteHeader = By.ClassName("complete-header");
        private static readonly By SubtotalLabel = By.ClassName("summary_subtotal_label");
        private static readonly By FirstNameField = By.Id("first-name");
        private static readonly By LastNameField = By.Id("last-name");
        private static readonly By PostalCodeField = By.Id("postal-code");
        private static readonly By ContinueButton = By.Id("continue");
        private static readonly By CartBadge = By.ClassName("shopping_cart_badge");
        private static readonly By CartItems = By.ClassName("cart_item");

        [SetUp]
        public void SetUp()
        {
                // Configure Firefox driver service for minimal logging
                var service = FirefoxDriverService.CreateDefaultService();
                service.HideCommandPromptWindow = true;
                service.SuppressInitialDiagnosticInformation = true;
                service.LogLevel = FirefoxDriverLogLevel.Fatal;

                var options = new FirefoxOptions();
                // options.AddArgument("--headless"); // Uncomment for headless mode
                driver = new FirefoxDriver(service, options);
            
        

            // Set implicit wait and maximize browser window
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
            driver.Manage().Window.Maximize();
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up by quitting and disposing driver
            driver?.Quit();
            driver?.Dispose();
        }

        public void Dispose() => driver?.Dispose();

        // Log in to the application with default or provided credentials
        private void Login(string user = "standard_user", string pass = "secret_sauce")
        {
            driver.Navigate().GoToUrl(BaseUrl);
            driver.FindElement(By.Id("user-name")).SendKeys(user);
            driver.FindElement(By.Id("password")).SendKeys(pass);
            driver.FindElement(By.Id("login-button")).Click();
            wait.Until(ExpectedConditions.UrlContains("/inventory.html"));
        }

        // Add an item to the cart by its ID
        private void AddToCart(string id) => driver.FindElement(By.Id($"add-to-cart-{id}")).Click();

        // Remove an item from the cart by its ID
        private void RemoveFromCart(string id) => driver.FindElement(By.Id($"remove-{id}")).Click();

        // Click an element and wait for URL to update
        private void ClickAndWait(By locator, string urlFragment)
        {
            wait.Until(ExpectedConditions.ElementExists(locator));
            driver.FindElement(locator).Click();
            wait.Until(ExpectedConditions.UrlContains(urlFragment));
        }

        // Navigate to the cart page
        private void GoToCart() => ClickAndWait(CartLink, "/cart.html");

        // Proceed to checkout step one
        private void GoToCheckout() => ClickAndWait(CheckoutButton, "/checkout-step-one.html");

        // Complete the checkout process
        private void FinishCheckout() => ClickAndWait(FinishButton, "/checkout-complete.html");

        // Fill in dummy checkout information
        private void FillCheckoutInfo(string fn = "Parth", string ln = "Thakkar", string pc = "M1G 3W4")
        {
            driver.FindElement(FirstNameField).SendKeys(fn);
            driver.FindElement(LastNameField).SendKeys(ln);
            driver.FindElement(PostalCodeField).SendKeys(pc);
            driver.FindElement(ContinueButton).Click();
            wait.Until(ExpectedConditions.UrlContains("/checkout-step-two.html"));
        }

        // Get text from an element located by the provided locator
        private string GetText(By by)
            => wait.Until(ExpectedConditions.ElementIsVisible(by)).Text;

        [Test, Category("UI")]
        public void Test1VerifyLoginNavigatesToProductsPage()
        {
            // Test that login redirects to the products page
            Login();
            Assert.That(GetText(TitleHeader), Is.EqualTo("Products"));
        }

        [Test, Category("UI")]
        public void Test2AddThreeItemsThenPurchaseTwo()
        {
            Login();
            AddToCart("sauce-labs-backpack");
            AddToCart("sauce-labs-bike-light");
            AddToCart("sauce-labs-bolt-t-shirt");

            GoToCart();
            RemoveFromCart("sauce-labs-bolt-t-shirt");

            // Verify exactly two items in cart
            var cartBadgeText = GetText(CartBadge);
            Assert.That(cartBadgeText, Is.EqualTo("2"), "Cart badge should show 2 items");
            var cartItemsCount = driver.FindElements(CartItems).Count;
            Assert.That(cartItemsCount, Is.EqualTo(2), "Cart should contain exactly 2 items");

            GoToCheckout();
            FillCheckoutInfo();
            FinishCheckout();

            // Confirm checkout completion
            Assert.That(GetText(CompleteHeader), Is.EqualTo("Thank you for your order!"));
            Assert.That(GetText(TitleHeader), Is.EqualTo("Checkout: Complete!"));
        }

        [Test, Category("UI")]
        public void Test3OrderValueBetween30And60()
        {
            Login();
            AddToCart("sauce-labs-backpack");
            AddToCart("sauce-labs-bike-light");

            GoToCart();
            GoToCheckout();
            FillCheckoutInfo();

            // Verify subtotal is within expected range
            var amount = double.Parse(
                GetText(SubtotalLabel).Split('$').Last(),
                CultureInfo.InvariantCulture
            );
            Assert.That(amount, Is.InRange(30.0, 60.0));

            FinishCheckout();
            
            // Confirm checkout completion
            Assert.That(GetText(TitleHeader), Is.EqualTo("Checkout: Complete!"));
            Assert.That(GetText(CompleteHeader), Is.EqualTo("Thank you for your order!"));
        }
    }
}