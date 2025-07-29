import { Routes } from '@angular/router';
import {MovieListComponent} from './reviewer/movie-list/movie-list.component';
import {MovieDetailsComponent as ReviewerMovieDetailsComponent} from './reviewer/movie-details/movie-details.component';
import {MovieDetailsComponent as PlannerMovieDetailsComponent} from './planner/movie-details/movie-details.component';
import {AddUserComponent} from './reviewer/add-user/add-user.component';
import {UserListComponent} from './planner/user-list/user-list.component';
import {WatchlistComponent} from './planner/watchlist/watchlist.component';
import {UserDetailsComponent} from './planner/user-details/user-details.component';

export const routes: Routes = [
  { path: 'reviewer/movies', component: MovieListComponent },
  { path: 'reviewer/movies/:movieId', component: ReviewerMovieDetailsComponent },
  { path: 'reviewer/users/new', component: AddUserComponent },
  { path: 'planner/users', component: UserListComponent, },
  { path: 'planner/users/:userId', component: UserDetailsComponent, },
  { path: 'planner/movies/:movieId', component: PlannerMovieDetailsComponent},
  { path: '**', redirectTo: 'reviewer/movies', pathMatch: 'full' }
];
