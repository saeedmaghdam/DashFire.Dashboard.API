
# DashFire.Dashboard.API
> DashFire is a .Net library to create jobs and services which could connect to DashFire Dashboard easily.

DashFire is totally free and open source .Net library to create jobs and services, schedule, log and manage them easily.

When you create a Worker Service in .Net, or Windows Service in .Net Framework, you'll not only care about the logic of the business, but about logging, managing, scheduling and etc. 

But using DashFire, you only implement the logic of the business, the issues like communicating with the server (DashFire Dashboard), logging, scheduling and other sort of issues will be handled by the DashFire.
Then, you can host the jobs in Services (in Windows) or Systemd (in Linux based operation systems).

DashFire.Dashboard.API is the server which jobs connect to it. The server stores the job's definitions and logs, user can execute a job using the dashboard as well, every job send the status to the server and the communications are handled by both library and server side.

### P.S. This library is an alternative to HangFire which is a professional task scheduling library used by million of people around the world. This project is not as generic as HangFire, but I wanted to make a fully customized task scheduler. This is a very fundamental version which handles the basics of communications and scheduling, for extending the library and server, you can easily fork the project and extend it.
	

## Installation

To install DashFire.Dashboard.API in Visual Studio's Package Manager Console:

```sh

Install-Package DashFire.Dashboard.API -Version 1.0.0

```

To install in a specific project use:

```sh

Install-Package DashFire.Dashboard.API -Version 1.0.0 -ProjectName Your_Project_Name

```

To update package use:

```sh

Update-Package DashFire.Dashboard.API

```

To update package in a specific project use:

```sh

Update-Package DashFire.Dashboard.API -ProjectName Your_Project_Name

```


Or visit DashFire.Dashboard.API's [Nuget][nuget-page] page to get more information.

## Usage example




## Release History
  
### Visit [CHANGELOG.md] to see full change log history of DashFire.Dashboard.API

* 1.0.0
	* Initialized the API
		* Executes the jobs
		* Stores the logs
		* Handles the heart-bit
		* Handles the registration

## Meta
Saeed Aghdam – [Linkedin][linkedin]

Distributed under the MIT license. See [``LICENSE``][github-license] for more information.

[https://github.com/saeedmaghdam/](https://github.com/saeedmaghdam/)

## Contributing

1. Fork it (<https://github.com/saeedmaghdam/DashFire.Dashboard.API/fork>)
2. Create your feature branch (`git checkout -b feature/your-branch-name`)
3. Commit your changes (`git commit -am 'Add a short message describes new feature'`)
4. Push to the branch (`git push origin feature/your-branch-name`)

5. Create a new Pull Request

<!-- Markdown link & img dfn's -->

[linkedin]:https://www.linkedin.com/in/saeedmaghdam/
[nuget-page]:https://www.nuget.org/packages/DashFire.Dashboard.API
[github]: https://github.com/saeedmaghdam/
[github-page]: https://github.com/saeedmaghdam/DashFire.Dashboard.API/
[github-license]: https://raw.githubusercontent.com/saeedmaghdam/DashFire.Dashboard.API/master/LICENSE
[CHANGELOG.md]: https://github.com/saeedmaghdam/DashFire.Dashboard.API/blob/master/CHANGELOG.md
[DashFire.Dashboard.API.Test]: https://github.com/saeedmaghdam/DashFire.Dashboard.API/tree/master/DashFire.Dashboard.API.Test