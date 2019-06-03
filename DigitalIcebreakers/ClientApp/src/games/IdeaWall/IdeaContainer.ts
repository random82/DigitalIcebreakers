import { clamp } from '../../util/clamp';
import * as PIXI from "pixi.js";
import { IdeaView } from './IdeaView';
import { Point } from './Point';

export class IdeaContainer extends PIXI.Container {
    app: PIXI.Application;
    ideaWidth: number;
    margin: number;
    pointerData: Point | undefined;
    ideaContainerDrag!: PIXI.Graphics;

    constructor(app: PIXI.Application, ideaWidth: number, margin: number) {
        super();

        this.app = app;
        
        this.ideaWidth = ideaWidth;

        this.margin = margin;

        this.setupDrag();

        this.pointerData = undefined;
    }

    setupDrag() {
        this.ideaContainerDrag = new PIXI.Graphics();
        this.ideaContainerDrag.interactive = true;
        this.ideaContainerDrag.beginFill(0xFF00000, 0)
        this.ideaContainerDrag.drawRect(0, 0, 1, 1);
        this.ideaContainerDrag.endFill();

        this.ideaContainerDrag.interactive = true;
        this.ideaContainerDrag.buttonMode = true;
        this.ideaContainerDrag.on('pointerdown', this.onDragStart);
        this.ideaContainerDrag.on('pointermove', this.onDragMove);
        this.ideaContainerDrag.on('pointerup', this.onDragEnd);
        this.ideaContainerDrag.on('pointerupoutside', this.onDragEnd);
    }

    getDragContainer() {
        return this.ideaContainerDrag;
    }

    onDragStart = (event: PIXI.interaction.InteractionEvent)  => {
        const point = event.data.getLocalPosition(this.app.stage);
        this.pointerData = { x: this.x - point.x, y: this.y - point.y };
    }

    onDragEnd = () => {
        this.pointerData = undefined;
    }

    onDragMove = (event: PIXI.interaction.InteractionEvent) => {
        if (this.pointerData) {
            const point = event.data.getLocalPosition(this.app.stage);
            const x = this.pointerData.x + point.x;
            const y = this.pointerData.y + point.y;
            const mostTop = Math.min(...this.children.map(p => this.app.screen.y - p.y))
            let mostLeft = Math.max(...this.children.map(p => p.x))
            const mostRight = Math.max(...this.children.map(p => this.app.screen.width - p.x - this.ideaWidth));
            const mostBottom = Math.max(...this.children.map(p => this.app.screen.height - p.y - this.ideaWidth));
            this.position.set(clamp(x, -mostLeft, mostRight), clamp(y, mostTop, mostBottom));
        }
    }

    resize() {
        this.ideaContainerDrag.width = this.app.screen.width;
        this.ideaContainerDrag.height = this.app.screen.height;
    }

    arrange() {
        this.position.set(0);
        const gap = this.ideaWidth + this.margin;
        let columns = Math.floor(this.app.screen.width / gap);

        if (columns * gap - this.margin + this.ideaWidth <= this.app.screen.width)
            columns++;

        this.children.forEach((c,  i) => {
            var row = Math.floor(i / columns)
            var column = Math.floor(i % columns);
            c.position.set(column * gap, row * gap);
            (c as IdeaView).onDragEnd(); // saves position
        })
    }
}