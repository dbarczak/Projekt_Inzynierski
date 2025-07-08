import { Injectable } from '@angular/core';
import * as signalR             from '@microsoft/signalr';
import { BehaviorSubject, Subject }      from 'rxjs';
import { Question } from './models/question.model';
import { BlockResultDto } from './models/evaluation.model';


@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hub!: signalR.HubConnection;

  private qs$ = new BehaviorSubject<Question[]>([]);
  public questions$ = this.qs$.asObservable();

  private count$ = new BehaviorSubject<number>(0);
  public submissionCount$ = this.count$.asObservable();

  private result$ = new BehaviorSubject<BlockResultDto|null>(null);
  public evaluationResult$ = this.result$.asObservable();

  private end$    = new Subject<void>();
  public  endExam$ = this.end$.asObservable();

  start() {
    this.hub = new signalR.HubConnectionBuilder()
      .withUrl(`https://localhost:7276/hubs/exam`, { withCredentials: true })
      .build();

    this.hub.start()
      .then(() => this.hub.invoke('JoinSession'))
      .catch(console.error);

    this.hub.on('ReceiveQuestions', (questions: Question[]) => this.qs$.next(questions));
    this.hub.on('SubmissionCountUpdated', (c: number)   => this.count$.next(c));
    this.hub.on('BlockEvaluated', (r: BlockResultDto) => this.result$.next(r));
    this.hub.on('ExamEnded', () => this.end$.next());
  }
}
