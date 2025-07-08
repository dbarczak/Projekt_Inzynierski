import { Component } from '@angular/core';
import { SignalRService } from '../signal-r.service';
import { QuestionService } from '../question.service';
import { Question } from '../models/question.model';
import { AnswerDto, BlockResultDto, ExaminerAnswerDto } from '../models/evaluation.model';
import { EvaluationService } from '../evaluation.service';
import { AuthService } from '../auth.service';

@Component({
  selector: 'app-examiner-page',
  standalone: false,
  templateUrl: './examiner-page.component.html',
  styleUrl: './examiner-page.component.css'
})
export class ExaminerPageComponent {
  questions: Question[] = [];
  blockNumber = 1;
  examinerCode: string | null = null;
  isLead = false;
  answers: AnswerDto[] = [];
  saved = false;
  result: BlockResultDto | null = null;

  constructor(
    private auth: AuthService,
    private signalR: SignalRService,
    private qs: QuestionService,
    private evalSvc: EvaluationService
  ) {}

  ngOnInit() {
    const saved = localStorage.getItem('exam.questions');
    if (saved) {
      try {
        this.questions = JSON.parse(saved);
        this.answers = this.questions.map(q => ({
          questionNumber:       q.questionNumber,
          knowledgeChecked:     false,
          understandingChecked: false,
          discussionChecked:    false
        }));
      } catch {
        localStorage.removeItem('exam.questions');
      }
    } else {
      this.qs.get(this.blockNumber).subscribe({
        next: qs => {
          this.questions = qs;
          localStorage.setItem('exam.questions', JSON.stringify(qs));
          this.answers = qs.map(q => ({
            questionNumber:       q.questionNumber,
            knowledgeChecked:     false,
            understandingChecked: false,
            discussionChecked:    false
          }));
        },
        error: () => {}
      });
    }

    this.signalR.start();
    this.signalR.questions$.subscribe(qs => {
      if (qs.length === 0) return;
      this.questions = qs;
      localStorage.setItem('exam.questions', JSON.stringify(qs));
      this.answers = qs.map(q => ({
        questionNumber:       q.questionNumber,
        knowledgeChecked:     false,
        understandingChecked: false,
        discussionChecked:    false
      }));
    });

    this.signalR.evaluationResult$.subscribe(r => this.result = r);

    this.signalR.endExam$.subscribe(() => {
      localStorage.removeItem('exam.questions');
      this.questions = [];
      this.result    = null;
      this.answers = [];
    });

    this.examinerCode = this.auth.getExaminerCode();
  }

  onChange() { this.saved = false; }

  save() {
    const dto: ExaminerAnswerDto = {
      blockNumber:    this.blockNumber,
      examinerCode:   this.examinerCode!,
      isLeadExaminer: this.isLead,
      answers:        this.answers
    };
    console.log(dto);
    this.evalSvc.submitExaminerAnswers(dto)
      .subscribe(() => this.saved = true);
  }
}
