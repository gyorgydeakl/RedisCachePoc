import {Component, inject, input, output} from '@angular/core';
import {CreateReviewDto, MovieReviewerClient, ReviewDto} from '../../../reviewer-client';
import {FormsModule} from '@angular/forms';
import {Textarea} from 'primeng/textarea';
import {ButtonDirective, ButtonIcon, ButtonLabel} from 'primeng/button';
import {Rating} from 'primeng/rating';
import {InputText} from 'primeng/inputtext';
import {Select} from 'primeng/select';
import {resourceObsNoParams} from '../../utils';

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
  readonly users = resourceObsNoParams(() => this.client.usersGet());
  command: CreateReviewDto = {
    rating: 0,
    title: '',
    description: '',
    userId: ''
  }
  selectLabel = "Select a user";

  createReview() {
    this.client.moviesIdReviewsPost(this.movieId(), this.command).subscribe(r => this.reviewAdded.emit(r))
  }
}
