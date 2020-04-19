import React from 'react';
import { Button } from '../pixi/Button';
import { BaseGameProps, BaseGame } from '../BaseGame'
import { Colors } from '../../Colors'
import { connect, ConnectedProps } from 'react-redux';
import { clientMessage } from '../../store/lobby/actions'
import { Pixi } from '../pixi/Pixi';

const connector = connect(
    null,
    { clientMessage }
);
  
type PropsFromRedux = ConnectedProps<typeof connector> & BaseGameProps;

export class SplatClient extends BaseGame<PropsFromRedux, {}> {
    private button: Button;
    app?: PIXI.Application;

    constructor(props: PropsFromRedux) {
        super(props);
        this.button = new Button(this.up, this.down);
    }

    init(app?: PIXI.Application) {
        if (app) {
            this.app = app;
        }
        
        if (this.app) {
            this.app.stage.addChild(this.button);
            this.button.x = this.app.renderer.width / 4;
            this.button.y = this.app.renderer.height / 4;
            this.button.render(Colors.Blue.C400, Colors.Red.C400, 0, 0, this.app.renderer.width / 2, this.app.renderer.height / 2);
        }
    }

    down = () => {
        this.props.clientMessage("down");
    }
    
    up = () => {
        this.props.clientMessage("up");
    }

    render() {
        return (
            <Pixi backgroundColor={Colors.BlueGrey.C400} onAppChange={(app) => this.init(app)} />
        );
    }
}

export default connector(SplatClient);