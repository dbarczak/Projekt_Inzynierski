import { Component, OnInit } from '@angular/core';
import { AdminService } from '../admin.service';
import { Question, QuestionSet, QuestionSetDetailDto } from '../models/question.model';
import { CodesResponseDto } from '../models/auth.model';
import { QuestionService } from '../question.service';
import { SignalRService } from '../signal-r.service';
import { EvaluationService } from '../evaluation.service';
import { AnswerDto, BlockResultDto, ExaminerAnswerDto, SubmitBlockDto } from '../models/evaluation.model';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-admin-page',
  standalone: false,
  templateUrl: './admin-page.component.html',
  styleUrl: './admin-page.component.css'
})
export class AdminPageComponent implements OnInit{
  selectedFile: File | null = null;
  uploadResponse: string | null = null;
  questions: Question[] = [];
  blocks: number[] = [];
  questionSets: QuestionSet[] = [];
  newSetName = '';
  selectedBlock: number | null = null;
  selectedSet: number | null = null;
  expectedExaminers = 1;
  submissionCount = 0;
  result: BlockResultDto|null = null;
  answers: AnswerDto[] = [];
  savedOwnEval = false;
  setDetails: QuestionSetDetailDto | null = null;
  showSet = false;
  examinersCount = 0;
  codes: CodesResponseDto | null = null;
  error: string | null = null;

  constructor(private http: HttpClient, private admin: AdminService, private signalR: SignalRService,
    private qs: QuestionService, private evalSvc: EvaluationService) {}

  ngOnInit(): void {
    this.signalR.start();
    this.signalR.questions$.subscribe(qs => {
      this.questions = qs;
      // za każdym razem gdy przyjdą pytania, przygotuj tabelkę ocen
      this.answers = qs.map(q => ({
        questionNumber: q.questionNumber,
        knowledgeChecked: false,
        understandingChecked: false,
        discussionChecked: false
      }));
      this.savedOwnEval = false;
    });
    this.signalR.submissionCount$.subscribe(c => this.submissionCount = c);
    this.signalR.evaluationResult$.subscribe(r => this.result = r);
    const saved = localStorage.getItem('exam.questions');
    if (saved) {
      try {
        this.questions = JSON.parse(saved);
        // odbuduj answers do panelu oceniania
        this.answers = this.questions.map(q => ({
          questionNumber:       q.questionNumber,
          knowledgeChecked:     false,
          understandingChecked: false,
          discussionChecked:    false
        }));
      } catch {
        localStorage.removeItem('exam.questions');
      }
    }
    const savedCodes = localStorage.getItem('exam.codes');
    if (savedCodes) {
      try {
        this.codes = JSON.parse(savedCodes) as CodesResponseDto;
        // jeśli chcesz, przywróć expectedExaminers
        this.expectedExaminers = this.codes.examinerCodes.length + 1;
      } catch {
        localStorage.removeItem('exam.codes');
      }
    }
    this.loadSets();
  }

  submitOwnEvaluation() {
    if (!this.selectedBlock) return;
    const dto: ExaminerAnswerDto = {
      blockNumber:    this.selectedBlock,
      examinerCode:   'admin',
      isLeadExaminer: true,
      answers:        this.answers
    };
    this.evalSvc.submitExaminerAnswers(dto)
      .subscribe(() => this.savedOwnEval = true);
  }

  onChange() { this.savedOwnEval = false; }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files?.length) {
      this.selectedFile = input.files[0];
      this.uploadResponse = null;
    }
  }

  onUpload(): void {
    if (!this.newSetName.trim() || !this.selectedFile) {
      this.uploadResponse = 'Podaj nazwę zestawu i wybierz plik.';
      return;
    }

    this.qs.uploadSet(this.newSetName.trim(), this.selectedFile)
    .subscribe({
      next: (msg: string) => {
        this.uploadResponse = msg;
        this.newSetName   = '';
        this.selectedFile = null;
      },
      error: err => {
        console.error(err);
        this.uploadResponse = 'Wystąpił błąd podczas wysyłania zestawu.';
      }
    });
  }

  endExam(): void {
    this.http.post('https://localhost:7276/Question/end', {})
      .subscribe(() => {
        localStorage.removeItem('exam.questions');
        localStorage.removeItem('exam.codes');
        this.questions = [];
        this.codes = null;
        this.savedOwnEval = false;
        this.answers = [];
        this.submissionCount = 0;
        this.selectedSet = null;
        this.selectedBlock = null;
        this.result = null;
        this.expectedExaminers = 1;
        this.examinersCount = 0;
      });
  }

  private loadSets(): void {
    this.admin.getSets().subscribe({
      next: qsets => this.questionSets = qsets, 
      error: err => console.error('Zestawy:', err)
    });
  }

  onSetChange(): void {
    if (this.selectedSet != null) {
      this.admin.getBlocks(this.selectedSet).subscribe(
        blks => {
          this.blocks = blks;
          this.selectedBlock = null;
        },
        err => {
          console.error('Bloki:', err);
          this.blocks = [];
        }
      );
    } else {
      this.blocks = [];
      this.selectedBlock = null;
    }
  }

  generateCodes(): void {
    this.error = null;
    this.codes = null;
    this.admin.generateCodes(this.examinersCount)
      .subscribe({
        next: c => {
          this.codes = c
          this.expectedExaminers = this.examinersCount + 1;
          localStorage.setItem('exam.codes', JSON.stringify(c));
        },
        error: err => {
          console.error('Generowanie kodów:', err);
          this.error = 'Nie udało się wygenerować kodów.';
        }
      });
  }

  generate() {
    this.showSet = false;  
    this.qs.generate(this.selectedSet!, this.selectedBlock!, 3)
    .subscribe({
      next: (questions) => {
        this.questions = questions;
        localStorage.setItem('exam.questions', JSON.stringify(questions));
      },
      error: err => {
        console.error('Błąd generowania pytań:', err);
      }
    });
  }


  onDeleteSet(): void {
    if (!this.selectedSet) return;
    
    const ok = confirm(
      `Czy na pewno chcesz usunąć zestaw "${this.questionSets.find(s=>s.id===this.selectedSet)?.name}"?`
    );
    if (!ok) return;

    this.qs.deleteSet(this.selectedSet).subscribe({
      next: () => {
        this.loadSets();

        this.blocks = [];
        this.selectedBlock = null;
        this.questions = [];
        this.uploadResponse = `Zestaw usunięty pomyślnie.`;
      },
      error: err => {
        console.error('Błąd usuwania zestawu:', err);
        this.uploadResponse = 'Nie udało się usunąć zestawu.';
      }
    });
  }

  showSetDetails(): void {
    if (!this.selectedSet) return;
    this.showSet = true; 
    this.qs.getSetDetails(this.selectedSet)
      .subscribe({
        next: dto => this.setDetails = dto,
        error:   () => this.uploadResponse = 'Nie udało się pobrać szczegółów.'
      });
  }

  canEvaluate() {
    return this.selectedBlock != null && this.submissionCount >= this.expectedExaminers;
  }

  evaluate() {
    if (!this.selectedBlock) return;
    this.evalSvc.evaluateBlock({ blockNumber: this.selectedBlock })
      .subscribe();
  }
}
