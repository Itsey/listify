namespace Listify.ITest;

using Listify.Model;
using Microsoft.Playwright;
using Plisky.Diagnostics;

public class Exploratory {
    Bilge b;
    // These are sample Integration tests to enable the CD pipeline to test deployment succeeded.
    string siteURL;

    public Exploratory() {
        b = new Bilge("integration-tests");
        b.Warning.Log("Hardcoded Configuration Path.");
        try {
            string pth = AppDomain.CurrentDomain.BaseDirectory;
            var lc = ListifyConfig.Create(pth, "1101");
            if (lc.AppSection?.PrimaryUrl == null) {
                throw new InvalidOperationException("PrimaryURL not configured, unable to test application without correct configuration.");
            }
            siteURL = lc.AppSection.PrimaryUrl;

        } catch (InvalidOperationException) {
            // The config files are not always copied correctly to the output directory.  As we only have one environment at the mo this workaround suffices.
            b.Error.Log("Unable to load configuration for integration tests - Workaround Established.");
            siteURL = "http://saspitsey-001-site4.dtempurl.com/";
        }
        Console.WriteLine($"Testing Url {siteURL}");
        b.Info.Log($"TestURL {siteURL}");
    }

    [Fact]
    public async Task Home_header_contains_expected_title() {
        var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync();
        var page = await browser.NewPageAsync();

        await page.GotoAsync(siteURL);
        Assert.Equal("Listify.Me - Listify", await page.TitleAsync());

        string? txt = await page.GetByTestId("home-title").TextContentAsync();
        Assert.StartsWith("Listify.Me", txt);
    }

    [Fact]
    public async Task Home_release_name_is_correct() {
        var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync();
        var page = await browser.NewPageAsync();

        await page.GotoAsync(siteURL);

        string? txt = await page.GetByTestId("home-releasename").TextContentAsync();
        Assert.StartsWith("👼Angel Release", txt);
    }

}
