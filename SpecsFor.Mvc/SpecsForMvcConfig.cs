using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using SpecsFor.Mvc.Authentication;
using SpecsFor.Mvc.IIS;
using SpecsFor.Mvc.Smtp;

namespace SpecsFor.Mvc
{
	public class SpecsForMvcConfig
	{
		public List<ITestRunnerAction> TestRunnerActions { get; private set; }

		public SpecsForMvcConfig()
		{
			TestRunnerActions = new List<ITestRunnerAction>();
		}

		private void AddNewAction(Action action)
		{
			TestRunnerActions.Add(new BasicTestRunnerAction(action, () => { }));
		}

		public void UseBrowser(BrowserDriver driver)
		{
			TestRunnerActions.Add(new BrowserDriverAction(driver));
		}

		public void BuildRoutesUsing(Action<RouteCollection> configAction)
		{
			AddNewAction(() => configAction(RouteTable.Routes));
		}

		public void RegisterArea<T>() where T : AreaRegistration, new()
		{
			AddNewAction(() =>
			             	{
								var reg = new T();
								reg.RegisterArea(new AreaRegistrationContext(reg.AreaName, RouteTable.Routes));
			             	});
		}

		public void Use<TConfig>() where TConfig : SpecsForMvcConfig, new()
		{
			TestRunnerActions.AddRange(new TConfig().TestRunnerActions);
		}

		public void BeforeEachTest(Action action)
		{
			AddNewAction(() => MvcWebApp.AddPreTestCallback(action));
		}

		public void InterceptEmailMessagesOnPort(int portNumber)
		{
			TestRunnerActions.Add(new SmtpIntercepterAction(portNumber));
		}

		public void AuthenticateBeforeEachTestUsing<TAuth>() where TAuth : IHandleAuthentication, new()
		{
			MvcWebApp.Authentication = new TAuth();
		}

		public IISExpressConfigBuilder UseIISExpress()
		{
			var builder = new IISExpressConfigBuilder();

			TestRunnerActions.Add(builder.GetAction());

			return builder;
		}

		public void UseApplicationAtUrl(string baseUrl)
		{
			AddNewAction(() => MvcWebApp.BaseUrl = baseUrl.TrimEnd('/'));
		}

		public void PostOperationDelay(TimeSpan delay)
		{
			AddNewAction(() => MvcWebApp.Delay = delay);
		}
	}

	public class BrowserDriverAction : ITestRunnerAction
	{
		private readonly BrowserDriver _driver;

		public BrowserDriverAction(BrowserDriver driver)
		{
			_driver = driver;
		}

		public void Startup()
		{
			MvcWebApp.Driver = _driver;
		}

		public void Shutdown()
		{
			_driver.Shutdown();
		}
	}
}