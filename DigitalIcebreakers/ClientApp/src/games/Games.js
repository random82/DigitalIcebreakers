import React from 'react';
import { DoggosVsKittehsClient } from './DoggosVsKittehs/DoggosVsKittehsClient';
import { DoggosVsKittehsPresenter } from './DoggosVsKittehs/DoggosVsKittehsPresenter';
import { BroadcastClient } from './Broadcast/BroadcastClient';
import { BroadcastPresenter } from './Broadcast/BroadcastPresenter';
import { YesNoMaybePresenter } from './YesNoMaybe/YesNoMaybePresenter';
import { YesNoMaybeClient } from './YesNoMaybe/YesNoMaybeClient';
import { IdeaWallClient } from './IdeaWall/IdeaWallClient';
import { IdeaWallPresenter } from './IdeaWall/IdeaWallPresenter';
import { BuzzerClient } from './Buzzer/BuzzerClient';
import { BuzzerPresenter } from './Buzzer/BuzzerPresenter';
import PongPresenter from './Pong/PongPresenter';
import { PongClient } from './Pong/PongClient';

export default function (props) {
    return [{
        name: "doggos-vs-kittehs",
        client: <DoggosVsKittehsClient {...props} />,
        presenter: <DoggosVsKittehsPresenter {...props} />,
        title: "Doggos vs Kittehs",
        isGame: true
    }, {
        name: "yes-no-maybe",
        client: <YesNoMaybeClient {...props} />,
        presenter: <YesNoMaybePresenter {...props} />,
        title: "Yes, No, Maybe"
    }, {
        name: "buzzer",
        client: <BuzzerClient {...props} />,
        presenter: <BuzzerPresenter {...props} />,
        title: "Buzzer",
        fullscreen: true
    }, {
        name: "pong",
        client: <PongClient {...props} />,
        presenter: <PongPresenter {...props} />,
        title: "Pong",
        fullscreen: true,
        isGame: true
    }, {
        name: "ideawall",
        client: <IdeaWallClient {...props} />,
        presenter: <IdeaWallPresenter {...props} />,
        title: "Idea Wall",
        fullscreen: true,
        isGame: false
    }, {
        name: "broadcast",
        client: <BroadcastClient {...props} />,
        presenter: <BroadcastPresenter {...props} />,
        title: "Broadcast",
        fullscreen: true,
        isGame: false
    }];
}
