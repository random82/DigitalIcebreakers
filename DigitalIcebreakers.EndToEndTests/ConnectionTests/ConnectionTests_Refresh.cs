using System.Threading.Tasks;
using Xunit;
using Shouldly;

namespace DigitalIcebreakers.EndToEndTests
{
    [Collection("Playwright")]
    public class ConnectionTests_Refresh : IAsyncLifetime
    {
        private readonly BrowserFactory _browsers;
        private Presenter _presenter;
        private Player _player;

        public ConnectionTests_Refresh(BrowserFactory browsers)
        {
            _browsers = browsers;
        }

        public async Task InitializeAsync()
        {
            _presenter = await _browsers.CreatePresenter();
            _player = await _browsers.CreatePlayer(_presenter.Url);
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public async Task Presenter_should_connect_after_refresh()
        {            
            await _presenter.Page.ReloadAsync();
            await Task.Delay(100);
            var connectionIcon = await _presenter.Page.GetByTestId("connection-status");
            var status = await connectionIcon.GetAttributeAsync("data-status");
            status.ShouldBe("Connected");
        }

        [Fact]
        public async Task Player_should_connect_after_refresh()
        {            
            await _player.Page.ReloadAsync();
            await Task.Delay(500);
            var connectionIcon = await _player.Page.GetByTestId("connection-status");
            var status = await connectionIcon.GetAttributeAsync("data-status");
            status.ShouldBe("Connected");
        }
    }
}
