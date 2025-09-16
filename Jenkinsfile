
pipeline {
    agent any

    environment {
        DOTNET_ROOT = "${HOME}/.dotnet"
        PATH = "${HOME}/.dotnet:${HOME}/.dotnet/tools:${env.PATH}"
        DOTNET_SYSTEM_GLOBALIZATION_INVARIANT = "1"
        REGISTRY = "demoapiregistry.azurecr.io"
        IMAGE_NAME = "demoapi"
        IMAGE_TAG = "latest"
    }

    stages {        
        stage('Install .NET') {
            steps {
                sh '''
                    curl -sSL -o install-dotnet.sh https://dot.net/v1/dotnet-install.sh
                    chmod +x install-dotnet.sh
                    ./install-dotnet.sh --channel 9.0 --install-dir $DOTNET_ROOT
                '''
            }
        }

        stage('Check .NET') {
            steps {
                sh 'dotnet --info'
            }
        }

        stage('Crear Tool Manifest') {
            steps {
                sh 'dotnet new tool-manifest --force'
                echo '✅ Tool manifest creado exitosamente.'
            }
        }

        stage('Instalar EF Core Tools') {
            steps {
                sh 'dotnet tool install dotnet-ef'
                echo '✅ EF Core Tools instalados exitosamente.'
            }
        }

        stage('Restaurar Dependencias') {
            steps {
                sh 'dotnet restore'
                sh 'dotnet tool restore'
                echo '✅ Dependencias restauradas exitosamente.'
            }
        }

        stage('Compilar') {
            steps {
                sh 'dotnet build --configuration Release --no-restore'
                echo '✅ Compilación exitosa.'
            }
        }

        stage('Ejecutar Pruebas') {
            steps {
                sh 'dotnet test --configuration Release --no-build --verbosity normal'
                echo '✅ Pruebas ejecutadas exitosamente.'
            }
        }

        stage('Publicar Artefactos') {
            steps {
                sh 'dotnet publish -c Release -o out'
                archiveArtifacts artifacts: 'out/**/*', fingerprint: true
                echo '✅ Artefactos publicados exitosamente.'
            }
        }

        stage('Login to ACR') {
            steps {
                withCredentials([usernamePassword(credentialsId: 'acr-creds',
                                                 usernameVariable: 'AZ_USER',
                                                 passwordVariable: 'AZ_PASS')]) {
                    sh """
                        echo $AZ_PASS | docker login $REGISTRY -u $AZ_USER --password-stdin
                    """
                }
            }
        }

        stage('Build Docker Image') {
            steps {
                sh """
                    docker build -t $REGISTRY/$IMAGE_NAME:$IMAGE_TAG .
                """
            }
        }

        stage('Push Docker Image') {
            steps {
                sh """
                    docker push $REGISTRY/$IMAGE_NAME:$IMAGE_TAG
                """
            }
        }
        
    }

    post {
        success {
            echo '✅ Pipeline completado con éxito.'
        }
        failure {
            echo '❌ Pipeline falló.'
        }
    }
}
