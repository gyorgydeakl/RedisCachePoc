import { Component, EventEmitter, Output, inject } from '@angular/core';
import { CreateMovieDto, MovieReviewerClient } from '../../client';
import { MessageService } from 'primeng/api';
import { FormsModule } from '@angular/forms';
import { InputText } from 'primeng/inputtext';
import { Textarea } from 'primeng/textarea';
import {Button, ButtonDirective, ButtonIcon, ButtonLabel} from 'primeng/button';

@Component({
  selector: 'app-add-movie',
  standalone: true,
  imports: [FormsModule, InputText, Textarea, ButtonIcon, ButtonDirective, ButtonLabel],
  templateUrl: './add-movie.component.html',
  styleUrl: './add-movie.component.css'
})
export class AddMovieComponent {
  private readonly client = inject(MovieReviewerClient);
  private readonly messageService = inject(MessageService);

  /** Notify parent when a movie is created */
  @Output() movieCreated = new EventEmitter<void>();

  command: CreateMovieDto = {
    title: '',
    genre: '',
    director: '',
    plot: ''
  };

  createMovie() {
    this.client.moviesPost(this.command).subscribe({
      next: movie => {
        this.messageService.add({
          text: `Movie '${movie.title}' added!`,
          severity: 'success',
        });
        this.movieCreated.emit();
        this.command = { title: '', genre: '', director: '', plot: '' };
      },
      error: _ =>
        this.messageService.add({
          text: `Error while creating movie.`,
          severity: 'error',
        })
    });
  }
}
