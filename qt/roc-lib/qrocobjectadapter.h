#ifndef QROCOBJECTADAPTER_H
#define QROCOBJECTADAPTER_H

#include <QObject>
#include <grpc++/grpc++.h>

class QRocObjectAdapter : public QObject
{
    Q_OBJECT
public:
    QRocObjectAdapter(QObject* parent = nullptr);
    ~QRocObjectAdapter();
    static void setSharedChannel(std::shared_ptr<grpc::Channel> channel);
    static std::shared_ptr<grpc::Channel> getSharedChannel();
private:
    static std::shared_ptr<grpc::Channel> _sharedChannel;
};

#endif // QROCOBJECTADAPTER_H
