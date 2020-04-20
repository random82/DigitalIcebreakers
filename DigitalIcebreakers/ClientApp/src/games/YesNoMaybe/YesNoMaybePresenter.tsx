import React, { Fragment, useState, useEffect } from 'react';
import Button from '../../layout/components/CustomButtons/Button';
import { Colors } from '../../Colors';
import { Graph } from '../pixi/Graph';
import { Pixi } from '../pixi/Pixi';
import { useDispatch } from 'react-redux';
import { adminMessage } from '../../store/lobby/actions';
import { useResizeListener } from '../pixi/useResizeListener';
import { useSelector } from '../../store/useSelector';

export const YesNoMaybeMenu = () => {
    const dispatch = useDispatch();
    const reset = () => {
        dispatch(adminMessage("reset"));
    }
    return (
        <Button onClick={reset}>Reset</Button>
    );
}

export default () => {
    const [pixi, setPixi] = useState<PIXI.Application>();
    const state = useSelector(state => state.games.yesnomaybe);

    const init = (app?: PIXI.Application) => {
        if (app) {
            setPixi(app);
        }
    };

    const resize = () => {
        const data = [
            {label: "Yes", value: state.yes, color: Colors.Red.C500},
            {label: "No", value: state.no, color: Colors.Blue.C500},
            {label: "Maybe", value: state.maybe, color: Colors.Grey.C500}
        ]
        if (pixi) {
            pixi.stage.removeChildren();
            new Graph(pixi, data)
        } else {
            console.log('no pixi');
        }
    } 
    
    useResizeListener(resize);
    useEffect(() => resize(), [pixi, state]);
    
    return <Pixi backgroundColor={0xFFFFFF} onAppChange={(app) => init(app)} />
}