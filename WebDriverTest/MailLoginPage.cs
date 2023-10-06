using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebDriverTest
{
    public class MailLoginPage
    {
        public IWebDriver driver;

        // Locators for elements 
        private readonly By emailLocator = By.Id("email");
        private readonly By passwordLocator = By.Id("password");
        private readonly By buttonLocator = By.ClassName("btn");
        private readonly By logoutButtonLocator = By.ClassName("account-info__logout");
        private readonly By rodoPopupContentLocator = By.ClassName("rodo-popup-content");
        private readonly By rodoPopupAgreeLocator = By.ClassName("rodo-popup-agree");
        private readonly By unreadMessageLocator = By.CssSelector("li.msglist-item:not(.msglist-item--seen)");
        private readonly By messageSenderInfo = By.CssSelector("span[ng-bind='::message.fromString']");


        public MailLoginPage(IWebDriver browser)
        {
            this.driver = browser;
        }

        public void NavigateTo(string url)
        {
            driver.Navigate().GoToUrl(url);
            CloseRodoPopupIfPresent();
        }

        private void CloseRodoPopupIfPresent()
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            try
            {
                wait.Until(ExpectedConditions.ElementIsVisible(rodoPopupContentLocator));
                driver.FindElement(rodoPopupAgreeLocator).Click();
                wait.Until(ExpectedConditions.InvisibilityOfElementLocated(rodoPopupContentLocator));
            }
            catch (WebDriverTimeoutException ex)
            {
                Console.WriteLine($"Timeout Exception: {ex.Message}");
            }
        }

        public void Login(string email, string password)
        {
            IWebElement emailField = driver.FindElement(emailLocator);
            IWebElement passwordField = driver.FindElement(passwordLocator);
            IWebElement buttonField = driver.FindElement(buttonLocator);

            emailField.SendKeys(email);
            passwordField.SendKeys(password);
            buttonField.Click();
        }

        public void Logout()
        {
            IWebElement buttonField = driver.FindElement(logoutButtonLocator);
            buttonField.Click();
        }

        public IWebElement FindUnreadMessageFromSender(string senderEmail)
        {
            var unreadMessages = driver.FindElements(unreadMessageLocator);

            foreach (var message in unreadMessages)
            {
                var senderInfo = message.FindElement(messageSenderInfo).GetAttribute("title");

                if (senderInfo.Contains(senderEmail))
                {
                    return message;
                }
            }
            return null;
        }
    }
}
