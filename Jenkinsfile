
pipeline {
    agent any

    environment {
        DB_HOST = 'localhost'
        DB_PORT = '5432'
        DB_NAME = 'pedidoDb'
        DOTNET_ROOT = "${HOME}/.dotnet"
        PATH = "${HOME}/.dotnet:${HOME}/.dotnet/tools:${env.PATH}"
        DOTNET_SYSTEM_GLOBALIZATION_INVARIANT = "1"
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

        stage('Actualizar Base de Datos') {
            steps {
                withCredentials([usernamePassword(credentialsId: 'db-credentials', usernameVariable: 'DB_USER', passwordVariable: 'DB_PASSWORD')]) {
                    withEnv(["CONNECTION_STRING=Host=${DB_HOST};Port=${DB_PORT};Database=${DB_NAME};Username=${DB_USER};Password=${DB_PASSWORD}"]) {
                        script {
                            try {
                                def output = sh(
                                    script: '''
                                        set -o pipefail
                                        set -x
                                        dotnet ef database update --connection "$CONNECTION_STRING" 2>&1 | tee /tmp/ef.log
                                    ''',
                                    returnStdout: true
                                ).trim()
                                echo "📄 Salida de EF:\n${output}"
                                echo "✅ Base de datos actualizada exitosamente."
                            } catch (err) {
                                echo "❌ Error al actualizar la base de datos"
                                sh 'cat /tmp/ef.log || true'
                                error "Falló la ejecución de dotnet ef"
                            }
                        }
                    }
                }
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
