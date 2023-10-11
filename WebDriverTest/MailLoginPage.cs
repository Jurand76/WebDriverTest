using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SeleniumExtras.PageObjects;
using OpenQA.Selenium.Support.Events;

namespace WebDriverTest
{
    public class MailLoginPage
    {
        public IWebDriver driver;

        // Fields for page 
        [FindsBy(How = How.Id, Using = "email")]
        private IWebElement emailField;

        [FindsBy(How = How.Id, Using = "password")]
        private IWebElement passwordField;

        [FindsBy(How = How.ClassName, Using = "btn")]
        private IWebElement loginButtonField;

        [FindsBy(How = How.ClassName, Using = "account-info__logout")]
        private IWebElement logoutButtonField;


        public MailLoginPage(IWebDriver browser)
        {
            this.driver = browser;
            PageFactory.InitElements(driver, this);
        }

        public void NavigateTo(string url)
        {
            driver.Navigate().GoToUrl(url);
            CloseRodoPopupIfPresent();
        }

        private void CloseRodoPopupIfPresent()
        {
            By rodoPopupContentLocator = By.ClassName("rodo-popup-content");
            By rodoPopupAgreeLocator = By.ClassName("rodo-popup-agree");
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            try
            {
                wait.Until(ExpectedConditions.ElementIsVisible(rodoPopupContentLocator));
                driver.FindElement(rodoPopupAgreeLocator).Click();
                wait.Until(ExpectedConditions.InvisibilityOfElementLocated(rodoPopupContentLocator));
            }
            catch (WebDriverTimeoutException ex)
            {
                Console.WriteLine($"Rodo popup - timeout exception: {ex.Message}");
            }
        }

        public void Login(string email, string password)
        {
            emailField.SendKeys(email);
            passwordField.SendKeys(password);
            loginButtonField.Click();
        }

        public void Logout()
        {
            logoutButtonField.Click();
        }

        public IWebElement FindUnreadMessageFromSender(string senderEmail)
        {
            By messageSenderInfo = By.CssSelector("span[ng-bind='::message.fromString']");
            By unreadMessagesLocator = By.CssSelector("li.msglist-item:not(.msglist-item--seen)");
            var unreadMessages = driver.FindElements(unreadMessagesLocator);

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
