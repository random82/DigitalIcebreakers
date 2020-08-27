import React, { useState, useEffect } from 'react';
import { useSelector } from '../../store/useSelector';
import { makeStyles } from '@material-ui/core/styles';
import IconButton from '@material-ui/core/IconButton';
import NavigateBefore from '@material-ui/icons/NavigateBefore';
import NavigateNext from '@material-ui/icons/NavigateNext';
import BarChart from '@material-ui/icons/BarChart';
import LiveHelp from '@material-ui/icons/LiveHelp';
import { Typography } from '@material-ui/core';
import { adminMessage } from '../../store/lobby/actions'
import { useDispatch } from 'react-redux';
import { setCurrentQuestionAction, toggleResponsesAction, currentQuestionSelector } from './PollReducer';
import Response from './components/Response';
import Button from '../../layout/components/CustomButtons/Button';
import { useHistory } from 'react-router-dom';

const useStyles = makeStyles(theme => ({
    root: {
        height: '100%',
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'center',
        flexDirection: 'column',
    },
    question: {
        margin: 0,
        padding: 0,
        textAlign: 'center',
    },
    responses: {
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'center',
        flexDirection: 'column',
        marginTop: 25,
    },
    buttons: {
        position: 'fixed',
        bottom:0,
        right:0,
        padding: '16px',
    }
}));

export default () => {
    const history = useHistory();
    const classes = useStyles();
    const dispatch = useDispatch();
    const {
        currentQuestionId,
        question
    } = useSelector(currentQuestionSelector);

    const { 
        questionIds,
        showResponses,
    } = useSelector(state => ({
        questionIds: state.games.poll.presenter.questions.map(q => q.id),
        showResponses: state.games.poll.presenter.showResponses,
    }));
    
    useEffect(() => {
        if (questionIds.length && !questionIds.find(f => currentQuestionId)) {
            dispatch(setCurrentQuestionAction(questionIds[0]));
        }
    }, [questionIds, currentQuestionId])

    const responses = useSelector(state => {
        if (question) {
            const q = state.games.poll.presenter.questions.find(q => q.id === question.id);
            if (q) {
                return q.responses.length;
            }
        }
        return 0;
    })
    const currentQuestionIndex = currentQuestionId ? questionIds.indexOf(currentQuestionId) : -1;
    const previousQuestionId = currentQuestionIndex > 0 ? questionIds[currentQuestionIndex-1] : null;
    const nextQuestionId = currentQuestionIndex != -1 && currentQuestionIndex < questionIds.length + 1 ? 
        questionIds[currentQuestionIndex+1] : null;

    const nextQuestion = () => nextQuestionId && dispatch(setCurrentQuestionAction(nextQuestionId));
    const previousQuestion = () => previousQuestionId && dispatch(setCurrentQuestionAction(previousQuestionId));
    
    useEffect(() => {
        if (question) {
            dispatch(adminMessage({
                questionId: question.id,
                answers: question.answers,
            }));
        }
        else{
            dispatch(adminMessage(null))
        }
    }, [currentQuestionId]);


    const QuestionView = () => {
        return (
            <>
                <h1 className={classes.question}>
                    {question!.text}
                </h1>
                <div className={classes.responses}>
                    <Typography variant='overline'>Responses</Typography>
                    <Typography>{responses}</Typography>
                </div>
            </>
        );
    }

    return (
        <>
            <div className={classes.root}>
                { question ? ( showResponses ? <Response /> : <QuestionView /> ) : (
                    <>
                        <h1 className={classes.question}>
                            No questions
                        </h1>
                        <Button color='primary' onClick={() => history.push('/questions')}>
                            Add some
                        </Button>
                    </>
                )}
            </div>
            <div className={classes.buttons}>
                <IconButton disabled={!previousQuestionId} onClick={() => previousQuestion()}>
                    <NavigateBefore />
                </IconButton>
                <IconButton onClick={() => dispatch(toggleResponsesAction())}>
                    { showResponses ? <LiveHelp /> : <BarChart /> }
                </IconButton>
                <IconButton disabled={!nextQuestionId} onClick={() => nextQuestion()}>
                    <NavigateNext />
                </IconButton>
            </div>
        </>
    )
}