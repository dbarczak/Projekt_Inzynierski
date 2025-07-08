import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { SignalRService } from '../signal-r.service';
import { Question } from '../models/question.model';
import { BlockResultDto } from '../models/evaluation.model';

@Component({
  selector: 'app-student-page',
  standalone: false,
  templateUrl: './student-page.component.html',
  styleUrl: './student-page.component.css'
})
export class StudentPageComponent implements OnInit {
  questions: Question[] = [];
  result: BlockResultDto | null = null;

  constructor(private signalR: SignalRService) {}

  ngOnInit(): void {
    const saved = localStorage.getItem('exam.questions');
    if (saved) {
      try {
        this.questions = JSON.parse(saved);
      } catch {
        localStorage.removeItem('exam.questions');
      }
    }
    this.signalR.start();

    this.signalR.questions$.subscribe(qs => {
      if (qs.length === 0) {
        return;
      }
      this.questions = qs;
      localStorage.setItem('exam.questions', JSON.stringify(qs));
    });

    this.signalR.evaluationResult$.subscribe(r => this.result = r);

    this.signalR.endExam$.subscribe(() => {
      localStorage.removeItem('exam.questions');
      this.questions = [];
      this.result    = null;
    });
  }
}
