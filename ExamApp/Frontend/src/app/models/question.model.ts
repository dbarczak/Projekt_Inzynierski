export interface Question {
    questionNumber: number;
    title: string;
    knowledge: string;
    understanding: string;
    discussion: string;
    examPart: number;
}

export interface QuestionSet {
  id: number;
  name: string;
  createdAt: string;
}

export interface QuestionSetDetailDto {
  name: string;
  createdAt: string;
  questions: Question[];
}