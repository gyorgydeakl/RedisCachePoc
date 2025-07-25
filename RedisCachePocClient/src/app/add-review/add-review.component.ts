import {Component, inject, input, output, signal} from '@angular/core';
import {CreateReviewDto, MovieReviewerClient, ReviewDto, UserDto} from '../../client';
import {FormsModule} from '@angular/forms';
import {Textarea} from 'primeng/textarea';
import {ButtonDirective, ButtonIcon, ButtonLabel} from 'primeng/button';
import {Rating} from 'primeng/rating';
import {InputText} from 'primeng/inputtext';
import {PrimeTemplate} from 'primeng/api';
import {Select} from 'primeng/select';

@Component({
  selector: 'app-add-review',
  imports: [
    FormsModule,
    Textarea,
    ButtonLabel,
    ButtonIcon,
    ButtonDirective,
    Rating,
    InputText,
    Select
  ],
  templateUrl: './add-review.component.html',
  styleUrl: './add-review.component.css'
})
export class AddReviewComponent {
  client = inject(MovieReviewerClient);

  movieId = input.required<string>();
  reviewAdded = output<ReviewDto>();
  users = signal<UserDto[]>([]);
  command: CreateReviewDto = {
    rating: 0,
    title: '',
    description: '',
    userId: ''
  }
  selectLabel = "Select a user";
  constructor() {
    this.client.usersGet().subscribe(users => this.users.set(users));
  }
  createReview() {
    this.client.moviesIdReviewsPost(this.movieId(), this.command).subscribe(r => this.reviewAdded.emit(r))
  }
}
