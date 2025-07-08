export interface AnswerDto {
  questionNumber: number;
  knowledgeChecked: boolean;
  understandingChecked: boolean;
  discussionChecked: boolean;
}

export interface ExaminerAnswerDto {
  blockNumber: number;
  examinerCode: string;
  isLeadExaminer: boolean;
  answers: AnswerDto[];
}

export interface SubmitBlockDto {
  blockNumber: number;
}

export interface BlockResultDto {
  knowledgeCount: number;
  understandingCount: number;
  discussionCount: number;
  passed: boolean;
  grade: number;
}