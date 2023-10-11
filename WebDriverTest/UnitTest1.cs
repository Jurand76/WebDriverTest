using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Chrome;
using SeleniumExtras.PageObjects;
//using SeleniumExtras.WaitHelpers;
using System;
using System.Threading;

namespace WebDriverTest
{
    public class Tests
    {
        public IWebDriver driver;
        public MailLoginPage mailLoginPage;
        public WebDriverWait wait;
       
        // URL for mail webpage and logins with passwords to 2 accounts
        private readonly string url = @"https://poczta.interia.pl/logowanie/";
        private readonly string email1 = "seleniumtest1@interia.pl";
        private readonly string email2 = "seleniumtest2@interia.pl";
        private readonly string password1 = "Testowekonto";
        private readonly string password2 = "Testowekonto";

         // Locators for interia.pl mail service
        private By iFrameLocator = By.CssSelector("iframe[id^='uiTinymce']");
        private By emailAddressLocator = By.CssSelector("input[ng-model='inputEmail']");
        private By emailEditArea = By.Id("mceu_15");
        private By emailContentLocator = By.Id("tinymce");
        private By sendButtonLocator = By.CssSelector("button[ng-click='sendButtonClickHandler()']");
        private By notificationMessageLocator = By.ClassName("notification__list");
        private By senderLocator = By.CssSelector("[title*='seleniumtest1@interia.pl']");
        private string senderAliasCorrect = "Selenium Test";
        private By dropdownIconLocator = By.CssSelector(".dropdown__icon.icon.icon-dots");
        private By replyButtonLocator = By.CssSelector("li.dropdown__item[ng-click='replyToSender(message)']");

      
        [SetUp]
        public void Setup()
        {
            driver = new ChromeDriver();
            mailLoginPage = new MailLoginPage(driver);
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        [Test, Order(0)]
        public void Test_Login_Failed()
        {
            By failedLoginLocator = By.ClassName("form__error");
            mailLoginPage.NavigateTo(url);
            mailLoginPage.Login(email2, "Badpassword");
            IWebElement failedLoginMessage = wait.Until(ExpectedConditions.ElementIsVisible(failedLoginLocator));
            string failedLoginText = failedLoginMessage.Text;
            Assert.IsTrue(failedLoginText.Contains("B³êdny e-mail"), "Bad answer from webpage");
        }

        [Test, Order(1)]
        public void Test_Login_Success()
        {
            By loginLocator = By.ClassName("msglist-header");
            mailLoginPage.NavigateTo(url);
            mailLoginPage.Login(email1, password1);
            IWebElement loginMessage = wait.Until(ExpectedConditions.ElementIsVisible(loginLocator));
            string loginText = loginMessage.Text;
            Assert.IsTrue(loginText.Contains("Odebrane"), "Login failed!");
        }

        [Test, Order(2)]
        public void Test_SendMail()
        {
            By newMessageButtonLocator = By.ClassName("navigation__new__text");

            mailLoginPage.NavigateTo(url);
            mailLoginPage.Login(email1, password1);
            
            // new message creation
            wait.Until(ExpectedConditions.ElementToBeClickable(newMessageButtonLocator)).Click();
            IWebElement emailField = wait.Until(ExpectedConditions.ElementIsVisible(emailAddressLocator));
            emailField.SendKeys("SeleniumTest2@interia.pl");

            // entering content of message
            IWebElement mailArea = wait.Until(ExpectedConditions.ElementToBeClickable(emailEditArea));
            mailArea.Click();
            wait.Until(ExpectedConditions.FrameToBeAvailableAndSwitchToIt(iFrameLocator));
            IWebElement bodyElement = wait.Until(ExpectedConditions.ElementIsVisible(emailContentLocator)); 
            bodyElement.Clear();  
            bodyElement.SendKeys("Testing - sending email to SeleniumTest2@interia.pl");
           
            driver.SwitchTo().DefaultContent();

            // sending message
            wait.Until(ExpectedConditions.ElementToBeClickable(sendButtonLocator)).Click();

            // checking if pop-up window with "Wiadomoœæ wys³ana" appeared
            IWebElement notificationMessage = wait.Until(ExpectedConditions.ElementIsVisible(notificationMessageLocator));
            string responseText = notificationMessage.Text;
            Assert.IsTrue(responseText.Contains("wys³ana"), "Sending message failed!");
        }


        [Test, Order(3)]
        public void Test_Received_Unread_Mail()
        {
            mailLoginPage.NavigateTo(url);
            mailLoginPage.Login(email2, password2);

            var messageToClick = mailLoginPage.FindUnreadMessageFromSender(email1);

            Assert.IsNotNull(messageToClick, "No new unread message from Selenium1 account");

            if (messageToClick != null) 
            {
                messageToClick.Click();
                IWebElement senderMessage = wait.Until(ExpectedConditions.ElementIsVisible(senderLocator));
                string senderAlias = senderMessage.Text;
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
                    wait.Until(ExpectedConditions.ElementToBeClickable(emailEditArea)).Click();
                    wait.Until(ExpectedConditions.FrameToBeAvailableAndSwitchToIt(iFrameLocator));
                    IWebElement bodyElement = wait.Until(ExpectedConditions.ElementIsVisible(emailContentLocator));
                    bodyElement.Clear();
                    bodyElement.SendKeys(senderAlias);

                    driver.SwitchTo().DefaultContent();

                    // sending message
                    wait.Until(ExpectedConditions.ElementToBeClickable(sendButtonLocator)).Click();

                    // checking if pop-up window with "Wiadomoœæ wys³ana" appeared
                    IWebElement notificationMessage = wait.Until(ExpectedConditions.ElementIsVisible(notificationMessageLocator));
                    string responseText = notificationMessage.Text;
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