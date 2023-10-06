using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Chrome;
using SeleniumExtras.PageObjects;
using SeleniumExtras.WaitHelpers;
using System;
using System.Threading;

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
                
              
        public MailLoginPage(IWebDriver browser, string url)
        {
            this.driver = browser;
            driver.Navigate().GoToUrl(url);
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            // close window with RODO rules
            wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("rodo-popup-content")));
            driver.FindElement(By.ClassName("rodo-popup-agree")).Click();
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.ClassName("rodo-popup-content")));
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
    }

    public class Tests
    {
        public IWebDriver driver;
        public MailLoginPage mailLoginPage;
       
        // URL for mail webpage and logins with passwords to 2 accounts
        private readonly string url = @"https://poczta.interia.pl/logowanie/";
        private readonly string email1 = "SeleniumTest1";
        private readonly string email2 = "SeleniumTest2";
        private readonly string password1 = "Testowekonto";
        private readonly string password2 = "Testowekonto";

        // Locators for interia.pl mail service
        private By loginLocator = By.ClassName("msglist-header");
        private By failedLoginLocator = By.ClassName("form__error");
        private By newMessageButtonLocator = By.ClassName("navigation__new__text");
        private By iFrameLocator = By.CssSelector("iframe[id^='uiTinymce']");
        private By emailAddressLocator = By.CssSelector("input[ng-model='inputEmail']");
        private By emailEditArea = By.Id("mceu_15");
        private By emailContentLocator = By.Id("tinymce");
        private By sendButtonLocator = By.CssSelector("button[ng-click='sendButtonClickHandler()']");
        private By notificationMessageLocator = By.ClassName("notification__list");
        private By receivedMessageClick = By.CssSelector("div[ng-click='openMessage(message)']");
        private By unreadMessageLocator = By.CssSelector("li.msglist-item:not(.msglist-item--seen)");
        private By messageSenderInfo = By.CssSelector("span[ng-bind='::message.fromString']");
        private By senderLocator = By.CssSelector("[title*='seleniumtest1@interia.pl']");
        private string senderAliasCorrect = "Selenium Test";
        private By dropdownIconLocator = By.CssSelector(".dropdown__icon.icon.icon-dots");
        private By replyButtonLocator = By.CssSelector("li.dropdown__item[ng-click='replyToSender(message)']");


        public string GetText(By locator_param)
        {
            WebDriverWait wait_answer = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            IWebElement pageElement = wait_answer.Until(ExpectedConditions.ElementIsVisible(locator_param));
            return pageElement.Text;
        }

        [SetUp]
        public void Setup()
        {
            driver = new ChromeDriver();
            mailLoginPage = new MailLoginPage(driver, url);
        }

        [Test, Order(0)]
        public void Test_Login_Failed()
        {
            mailLoginPage.Login(email2, "Badpassword");
            string failedLoginText = GetText(failedLoginLocator);
            Assert.IsTrue(failedLoginText.Contains("B³êdny e-mail"), "Bad answer from webpage");
        }

        [Test, Order(1)]
        public void Test_Login_Success()
        {
            mailLoginPage.Login(email1, password1);
            string loginText = GetText(loginLocator);
            Assert.IsTrue(loginText.Contains("Odebrane"), "Login failed!");
        }

        [Test, Order(2)]
        public void Test_SendMail()
        {
            mailLoginPage.Login(email1, password1);
            
            // new message creation
            IWebElement buttonField = driver.FindElement(newMessageButtonLocator);
            buttonField.Click();
            WebDriverWait wait_answer = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            IWebElement emailField = wait_answer.Until(ExpectedConditions.ElementIsVisible(emailAddressLocator));
            emailField.SendKeys("SeleniumTest2@interia.pl");

            // entering content of message
            IWebElement mailArea = wait_answer.Until(ExpectedConditions.ElementIsVisible(emailEditArea));
            Thread.Sleep(2000);
            mailArea.Click();
            wait_answer.Until(ExpectedConditions.FrameToBeAvailableAndSwitchToIt(iFrameLocator));
            IWebElement bodyElement = wait_answer.Until(ExpectedConditions.ElementIsVisible(emailContentLocator)); 
            bodyElement.Clear();  
            Thread.Sleep(2000);
            bodyElement.SendKeys("Testing - sending email to SeleniumTest2@interia.pl");
           
            driver.SwitchTo().DefaultContent();

            // sending message
            wait_answer.Until(ExpectedConditions.ElementToBeClickable(sendButtonLocator));
            IWebElement sendButton = driver.FindElement(sendButtonLocator);
            Thread.Sleep(3000);
            sendButton.Click();

            // checking if pop-up window with "Wiadomoœæ wys³ana" appeared
            Thread.Sleep(1000);
            string responseText = GetText(notificationMessageLocator);
            Assert.IsTrue(responseText.Contains("wys³ana"), "Sending message failed!");
        }

        [Test, Order(3)]
        public void Test_Received_Unread_Mail()
        {
            mailLoginPage.Login(email2, password2);
            
            var unreadMessages = driver.FindElements(unreadMessageLocator);
            var unreadFromSelenium1 = false;
            IWebElement messageToClick = null;

            // checking all messages for unread ones - and looking for mail from: seleniumtest1@interia.pl

            foreach (var message in unreadMessages)
            {
                var senderInfo = message.FindElement(messageSenderInfo).GetAttribute("title");

                if (senderInfo.ToString().Contains("seleniumtest1@interia.pl"))
                {
                    if (!unreadFromSelenium1)
                    {
                        messageToClick = message;
                        unreadFromSelenium1 = true;
                    }
                }                                        
            }

            Assert.IsTrue(unreadFromSelenium1, "No new unread message from Selenium1 account");

            if (messageToClick != null) 
            {
                messageToClick.Click();
                string senderAlias = GetText(senderLocator);
                Console.WriteLine("Sender alias: " + senderAlias);
                Assert.IsTrue(senderAlias.ToLower() == senderAliasCorrect.ToLower(), "Bad alias name of sender");

                if (senderAlias.ToLower() == senderAliasCorrect.ToLower())
                {
                    WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                    IWebElement iconButton = wait.Until(ExpectedConditions.ElementToBeClickable(dropdownIconLocator));
                    iconButton.Click();
                    IWebElement replyButton = wait.Until(ExpectedConditions.ElementToBeClickable(replyButtonLocator));
                    replyButton.Click();

                    // entering content of message
                    IWebElement mailArea = wait.Until(ExpectedConditions.ElementIsVisible(emailEditArea));
                    Thread.Sleep(2000);
                    mailArea.Click();
                    wait.Until(ExpectedConditions.FrameToBeAvailableAndSwitchToIt(iFrameLocator));
                    IWebElement bodyElement = wait.Until(ExpectedConditions.ElementIsVisible(emailContentLocator));
                    bodyElement.Clear();
                    Thread.Sleep(2000);
                    bodyElement.SendKeys(senderAlias);

                    driver.SwitchTo().DefaultContent();

                    // sending message
                    wait.Until(ExpectedConditions.ElementToBeClickable(sendButtonLocator));
                    IWebElement sendButton = driver.FindElement(sendButtonLocator);
                    Thread.Sleep(3000);
                    sendButton.Click();

                    // checking if pop-up window with "Wiadomoœæ wys³ana" appeared
                    Thread.Sleep(1000);
                    string responseText = GetText(notificationMessageLocator);
                    Assert.IsTrue(responseText.Contains("wys³ana"), "Sending answer to message failed!");
                }
            }
        }

        [TearDown]
        public void TearDown()
        {
            driver.Quit();
        }
    }
}