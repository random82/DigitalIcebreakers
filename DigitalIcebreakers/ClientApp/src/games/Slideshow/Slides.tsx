import React, {Component} from "react"
import "./slides.css";
import {fitRectIntoBounds} from "./fitRectIntoBounds";

interface SlidesState {
	width: number;
	height: number;
}

export class Slides extends Component<{}, SlidesState> {
	constructor(props: any) {
		super(props);
		this.state = {
			width: 600,
			height: 480
		};
	}

	theElement = (element: HTMLDivElement) => {
		if (element) {
			const result = fitRectIntoBounds(
				{width: 1520, height: 680}, 
				{width: element.clientWidth, height: element.clientHeight}
			);
			this.setState(result);
		}
    }

    render() {
		const containerStyle: React.CSSProperties = {
			position:"relative", 
			width: `${this.state.width}px`, 
			height: `${this.state.height}px`, 
			margin:"0 auto"
		};

        return (
            <div className="slides" ref={this.theElement}>
				<section>
					<h1>How do real-time apps work?</h1>
				</section>
				<section data-background-color="#ffffff">
					<div style={containerStyle}>
						<img width={this.state.width} height={this.state.height} src="img/200-longpolling.001.png" style={{position:"absolute", top: 0, left: 0}} />
						<img width={this.state.width} height={this.state.height} src="img/200-longpolling.002.png" style={{position:"absolute", top: 0, left: 0}} className="fragment" />
						<img width={this.state.width} height={this.state.height} src="img/200-longpolling.003.png" style={{position:"absolute", top: 0, left: 0}} className="fragment" />
						<img width={this.state.width} height={this.state.height} src="img/200-longpolling.004.png" style={{position:"absolute", top: 0, left: 0}} className="fragment" />
					</div>
				</section>
				<section data-background-color="#ffffff">
					<h2>Server-Sent Events</h2>
					<ul>
						<li className="fragment">HTML5</li>
						<li className="fragment">One-way communication</li>
						<li className="fragment">Low latency</li>
						<li className="fragment">Unsupported by Edge &amp; IE</li>
					</ul>
				</section>
				<section data-background-color="#ffffff">
					<h2>&lt;sse diagram&gt;</h2>
				</section>
				<section data-background-color="#ffffff">
					<h2>Web Socket</h2>
					<ul>
						<li className="fragment">Bi-directional</li>
					</ul>
				</section>
				<section data-background-color="#ffffff">
					<h3>Which transport method<br/>is the best?</h3>
				</section>
				<section>
					<pre><code className="csharp" data-trim>{`
						public void ConfigureServices(IServiceCollection services)
						{
						  services.AddSignalR();
						}

						public void Configure(IApplicationBuilder app)
						{
						  app.UseRouting();
						  app.UseEndpoints(endpoints =>
						  {
						    endpoints.MapHub<BidHub>("/echo");
						  });
						}`}
					</code></pre>
				</section>
				<section>
					<pre><code data-trim>{`
						public class BidHub : Hub
						{
						  public async Task Send(string message)
						  {
						    await Clients.All.SendAsync("BroadCastClient", message);
						  }
						}
					`}</code></pre>
				</section>
				<section data-background-color="#ffffff">
				<h2>&lt;deployment diagram&gt;</h2>
				</section>
				<section>
					<pre><code>
						npm install @microsoft/signalr	
					</code></pre>
				</section>
				<section>
					<pre><code className='javascript' data-trim>{`
						constructor() {    
						  this.hubConnection = new signalR.HubConnectionBuilder()
						    .withUrl("https://serverbidding.azurewebsites.net/bid")
						    .build();
						}

						sendMessage() {
						  this.hubConnection.send("send","send this to server")}
						}
						`}</code></pre>
				</section>
				<section>
					<pre><code className='javascript' data-trim>{`
						constructor() {
						  this.hubConnection.on("BroadCastClient",
						    this.broadcastCallBack);
						}   
						
						broadcastCallBack(name, message) {
						  alert(message);
						}
					`}</code></pre>
				</section>
				<section>
					<h1>Pushing<br/>the limit</h1>
				</section>
				<section data-background-color="#ffffff">
					<h2>Crankier</h2>
				</section>
				<section data-background-color="#ffffff">
					<table>
						<tr>
							<td>Local</td><td>&gt;&gt;</td><td>S1 App<br/>Service</td>
						</tr>
						<tr>
							<td colSpan={3} style={{textAlign:'center'}}>768 connections</td>
						</tr>
					</table>
				</section>
			</div>
        );
    }
}