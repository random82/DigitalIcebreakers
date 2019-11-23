﻿import React, { Component } from 'react';
import { Link } from 'react-router-dom';
import { Glyphicon, Nav, Navbar, NavItem } from 'react-bootstrap';
import { LinkContainer } from 'react-router-bootstrap';
import './NavMenu.css';
import { Config } from '../config';
import { NavSubMenu } from './NavSubMenu'
import Games, { IGame } from '../games/Games';
import { Events } from '../Events';
import { ConnectionStatus } from '../ConnectionStatus';
import { History } from 'history';

var QRCode = require('qrcode.react');

type NavMenuState = {
    qrCodeWidth: number;
}

type NavMenuProps = {
    connectionStatus: ConnectionStatus
    lobbyId?: string;
    toggleMenu: (show: boolean) => void;
    currentGame?: string;
    isAdmin: boolean;
    history: History;
    menuItems: JSX.Element[];
    version: string;
}

export class NavMenu extends Component<NavMenuProps, NavMenuState> {
  displayName = NavMenu.name
  private qrContainerElement?: HTMLElement;

    constructor(props: NavMenuProps, context: any) {
        super(props, context);
        this.state = {
            qrCodeWidth: 256
        };
    }

    componentDidMount() {
        const resize = () => {
            this.setState({qrCodeWidth: window.innerWidth / 4 - 60});
        }
        resize();
        Events.add('onresize', 'qrcode-lobby', resize);
    }

    componentWillUnmount() {
        Events.remove('onresize', 'qrcode-lobby');
    }

    getConnectionIcon() {
        switch (this.props.connectionStatus) {
            case 0: return (<Glyphicon glyph="remove-sign" />);
            case 1: return (<Glyphicon glyph="question-sign" />);
            case 2: return (<Glyphicon glyph="ok-sign" />);
            default: return "";
        }
    }

    qrContainer = (e: any) => {
        this.qrContainerElement = e;      
    }

    collapse = () => {
        this.props.toggleMenu(false);
    }

    render() {
        const joinUrl = `${Config.baseUrl}/join/${this.props.lobbyId}`;

        const createLobby = (
            <LinkContainer to={'/createLobby'}>
                <NavItem>
                    <Glyphicon glyph='plus' /> Host
                </NavItem>
            </LinkContainer>);

        const lobby = (
            <LinkContainer to={'/'} exact>
                <NavItem>
                    <Glyphicon glyph='home' /> Lobby
              </NavItem>
            </LinkContainer>
        );

        const closeLobby = (
            <LinkContainer to={'/closeLobby'}>
                <NavItem>
                    <Glyphicon glyph='remove' /> Close Lobby
                </NavItem>
            </LinkContainer>
        );

        const startGame = (
            <LinkContainer to={'/newGame'}>
                <NavItem>
                    <Glyphicon glyph='play' /> New game
                </NavItem>
            </LinkContainer>
        );

        const collapseNav = (
            <NavItem eventKey={1} onClick={() => this.collapse()}>
                <Glyphicon glyph='circle-arrow-left' />Hide Menu
            </NavItem>
        );

        const gameName = Games(this.props)
            .filter((game: any) => game.name === this.props.currentGame)
            .map((game: IGame) => game.title);

        const currentGame = (
            <LinkContainer to={`/game/${this.props.currentGame}`} exact>
                <NavItem>
                    <Glyphicon glyph='king' /> {gameName}
                </NavItem>
            </LinkContainer>
        );

        const qrCode = (
            <NavItem ref={this.qrContainer}>
                <QRCode value={joinUrl} size={this.state.qrCodeWidth} renderAs="svg" />
            </NavItem>
        );

        const isGameScreen = this.props.history.location.pathname === `/game/${this.props.currentGame}`;
        const isLobby = this.props.history.location.pathname === `/`

        const subMenu = !isGameScreen ? "" : (
            <NavSubMenu menuItems={this.props.menuItems} />
        );

        return (

            <Navbar inverse fixedTop fluid collapseOnSelect>
                <Navbar.Header>
                    <Navbar.Brand>
                        <Link to={'/'}>Digital Icebreakers</Link>
                    </Navbar.Brand>
                    <Navbar.Toggle />
                </Navbar.Header>
                <Navbar.Collapse>
                    <Nav>
                        {this.props.lobbyId && !isLobby ? qrCode : ""}
                        {this.props.lobbyId ? "" : createLobby}
                        {this.props.lobbyId ? lobby : ""}
                        {this.props.lobbyId && this.props.currentGame ? currentGame : ""}
                        {subMenu}
                        {this.props.lobbyId && this.props.isAdmin ? startGame : ""}
                        {this.props.currentGame && this.props.isAdmin && !isLobby ? collapseNav : ""}
                        {this.props.lobbyId && this.props.isAdmin ? closeLobby : ""}
                        <NavItem disabled={true}>
                            {this.getConnectionIcon()} Connection - v{this.props.version}
                        </NavItem>
                    </Nav>
                </Navbar.Collapse>
            </Navbar>
        );
  }
}